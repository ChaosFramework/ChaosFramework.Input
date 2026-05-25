using System;
using System.Linq;
using System.Threading;
using ChaosFramework.Collections;
using MeasurementLog = ChaosUtil.Debug.MeasurementLog;
using SysCol = System.Collections.Generic;

namespace ChaosFramework.Input
{
    using Layouts;

    public class InputContext : Core.Disposable
    {
        internal enum Dummy { Dummy }

        public readonly LayoutManager layoutMgr;

        readonly object queueLock = new object();

        readonly int numLayers;

        readonly LinkedList<InputDevice> allDevices = [];
        readonly Thread updateThread;
        readonly AutoResetEvent initEvent = new(false);

        LinkedList<InputEvent> queuedEvents = new();
        readonly Func<InputContext, InputDeviceHost> getOrCreateHost;
        InputDeviceHost deviceHost;

        /// <summary>
        ///     <list type="bullet">
        ///         <item>K1: Generic type definition of event.</item>
        ///         <item>K2: Concrete specialization of handled event type.</item>
        ///         <item>V: The handler.</item>
        ///     </list>
        /// </summary>
        readonly SysCol.Dictionary<Type, SysCol.Dictionary<Type, LinkedList<Delegate>[]>> eventTypeHandlers = [];

        public InputContext(Type consumeLayerEnum, Func<InputContext, InputDeviceHost> getOrCreateHost)
        {
            this.getOrCreateHost = getOrCreateHost;
            if (!consumeLayerEnum.IsEnum)
                throw new ArgumentException($"{nameof(consumeLayerEnum)} must be an enum.", nameof(consumeLayerEnum));

            numLayers = consumeLayerEnum.GetEnumValues().Length;

            layoutMgr = new LayoutManager();
            updateThread = new Thread(InputThread);
            updateThread.Start();
            initEvent.WaitOne();
        }

        public void AddHandler<EventType, AxisType, EventData>(Enum layer, Func<EventType, bool> handler)
            where AxisType : InputAxis
            where EventType : InputEvent<AxisType, EventData>
        {
            AssertAlive();

            Type evtType = GetSpecializedEventTypeDefinition(typeof(EventType));
            Type axisType = evtType.BaseType.GetGenericArguments()[0];

            if (axisType == null)
                throw new ArgumentException($"{typeof(EventType)} must accept its {nameof(AxisType)} as first type argument.");

            SysCol.Dictionary<Type, LinkedList<Delegate>[]> handlers
                = eventTypeHandlers.GetOrCreateValue(evtType.GetGenericTypeDefinition());

            if (!handlers.TryGetValue(axisType, out LinkedList<Delegate>[] layers))
                handlers[axisType] = layers = new LinkedList<Delegate>[numLayers];

            (layers[(int)(Dummy)layer] ?? (layers[(int)(Dummy)layer] = new LinkedList<Delegate>())).Add(handler);
        }

        protected internal void AddEvent(InputEvent e)
        {
            lock (queueLock)
                queuedEvents.Add(e);
        }

        public void UpdateDeviceList()
        {
            MeasurementLog.StartMeasure("Update InputDevice List");
            lock (updateThread)
            {
                allDevices.Clear();
                allDevices.Add(deviceHost.RefreshDeviceList());
            }
            MeasurementLog.EndMeasure();
        }

        Type GetSpecializedEventTypeDefinition(Type t)
        {
            while (t != null)
            {
                Type baseType = t.BaseType;
                if (baseType == null)
                    throw new ArgumentException($"Handled event types must inherit {nameof(InputEvent)}<,>.");

                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(InputEvent<,>))
                    break;

                t = baseType;
            }

            return t;
        }

        MeasurementLog.CustomAttribute[] NumDevicesCustomAttr(double t)
            => [new MeasurementLog.CustomAttribute("Number of Devices", allDevices.length.ToString())];

        /// <summary>
        ///     Processes all events that have been raised since the previous call to this function
        ///     and optionally updates the consistent state of all devices.
        /// </summary>
        /// <param name="updateDeviceState"> Whether to update device state. </param>
        /// <remarks>
        ///     "Consistent state" implies
        ///     that all inquiries of <see cref="InputAxis.value"/> and dependent properties shall yield identical results
        ///     between two calls of <see cref="UpdateInputConsumption(bool)"/>
        ///     with <paramref name="updateDeviceState"/> == <see langword="true"/>.
        /// </remarks>
        public void UpdateInputConsumption(bool updateDeviceState = true)
        {
            MeasurementLog.StartMeasure(nameof(UpdateInputConsumption));
            LinkedList<InputEvent> events;
            lock (queueLock)
            {
                MeasurementLog.StartMeasure("Device Update");
                events = queuedEvents;
                queuedEvents = new LinkedList<InputEvent>();
                if (updateDeviceState)
                    foreach (InputDevice dev in EnumerateDevices())
                        dev.AdvanceFrame();

                MeasurementLog.EndMeasure(NumDevicesCustomAttr);
            }

            MeasurementLog.StartMeasure($"Events ({events.length})");

            foreach (InputEvent e in events)
            {
                Type specializedEventType = GetSpecializedEventTypeDefinition(e.GetType());
                Type unboundEventType = specializedEventType.GetGenericTypeDefinition();

                if (eventTypeHandlers.TryGetValue(unboundEventType, out SysCol.Dictionary<Type, LinkedList<Delegate>[]> matchingAxes))
                {
                    Type raisedAxis = specializedEventType.BaseType.GetGenericArguments()[0];
                    for (Type handledAxis = raisedAxis; handledAxis != typeof(object); handledAxis = handledAxis.BaseType)
                        if (matchingAxes.TryGetValue(handledAxis, out LinkedList<Delegate>[] concreteAxisHandlers))
                            foreach (LinkedList<Delegate> inputLayer in concreteAxisHandlers)
                            {
                                // TODO: support currying further type arguments
                                InputEvent handledEvent = (InputEvent)Activator.CreateInstance(
                                    unboundEventType.MakeGenericType(handledAxis),
                                    [e.axisInternal, e.dataInternal]
                                    );

                                if (inputLayer != null)
                                    foreach (Delegate handler in inputLayer)
                                        try
                                        {
                                            MeasurementLog.StartMeasure($"Handler '{handler}'");
                                            if ((bool)handler.DynamicInvoke(handledEvent))
                                                goto consumed;
                                        }
                                        finally
                                        {
                                            MeasurementLog.EndMeasure();
                                        }
                            }
                }
            consumed:;
            }
            MeasurementLog.EndMeasure();

            eventTypeHandlers.Clear();
            MeasurementLog.EndMeasure();
        }

        void InputThread()
        {
            deviceHost = getOrCreateHost(this);
            initEvent.Set();
            while (alive)
            {
                lock (updateThread)
                {
                    deviceHost.Update();
                }
            }
        }

        public T GetDevice<T>(int index = 0)
            where T : InputDevice
            => (T)GetDevice(typeof(T), index);

        public InputDevice GetDevice(Type deviceType, int index = 0)
        {
            int i = 0;
            foreach (InputDevice dev in allDevices)
                if (dev.GetType() == deviceType)
                    if (i++ == index)
                        return dev;

            return null;
        }

        public SysCol.IEnumerable<InputDevice> EnumerateDevices()
            => allDevices.Select(Linq.SelectIdentity);

        public SysCol.IEnumerable<DeviceType> EnumerateDevices<DeviceType>()
            where DeviceType : InputDevice
            => EnumerateDevices().OfType<DeviceType>();

        protected override void DoDispose()
        {
            base.DoDispose();
            updateThread.Join();
            eventTypeHandlers.Clear();
        }
    }
}
