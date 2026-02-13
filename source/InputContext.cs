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

        readonly int numLayers;

        readonly LinkedList<InputDevice> allDevices = new LinkedList<InputDevice>();
        readonly Thread updateThread;
        readonly AutoResetEvent initEvent = new AutoResetEvent(false);

        LinkedList<InputEvent> queuedEvents = new LinkedList<InputEvent>();
        Func<InputContext, InputDeviceHost> getOrCreateHost;
        InputDeviceHost deviceHost;

        /// <summary>
        ///     <list type="bullet">
        ///         <item>K1: Generic type definition of event.</item>
        ///         <item>K2: Concrete specialization of handled event type.</item>
        ///         <item>V: The handler.</item>
        ///     </list>
        /// </summary>
        SysCol.Dictionary<Type, SysCol.Dictionary<Type, LinkedList<Delegate>[]>> eventTypeHandlers = new();

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

        void Register<EventType, AxisType, EventData>(Enum layer, Func<EventType, bool> handler)
            where EventType : InputEvent
        {
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

            (layers[(int)(Dummy)layer] ??= []).Add(handler);
        }

        protected internal void AddEvent(InputEvent e)
        {
            lock (queuedEvents)
                queuedEvents.Add(e);
        }

        public void UpdateDeviceList()
        {
            MeasurementLog.StartMeasure("Update InputDevice List");
            lock (updateThread)
            {
                foreach (InputDevice dev in deviceHost)
                    allDevices.Remove(dev);

                deviceHost.RefreshDeviceList();
                allDevices.Add(deviceHost);
            }
            MeasurementLog.EndMeasure();
        }

        Type GetSpecializedEventTypeDefinition(Type t)
        {
            while(t != null)
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

        public void UpdateInputConsumption(bool updateDevices = true)
        {
            MeasurementLog.StartMeasure(nameof(UpdateInputConsumption));
            LinkedList<InputEvent> events;
            lock (queuedEvents)
            {
                MeasurementLog.StartMeasure("Device Update");
                events = queuedEvents;
                queuedEvents = new LinkedList<InputEvent>();
                if (updateDevices)
                    foreach (InputDevice dev in EnumerateDevices())
                        dev.Update();

                MeasurementLog.EndMeasure(NumDevicesCustomAttr);
            }

            MeasurementLog.StartMeasure($"Events ({events.length})");

            foreach (InputEvent e in events)
            {
                Type specializedEventType = GetSpecializedEventTypeDefinition(e.GetType());
                Type unboundEventType = specializedEventType.GetGenericTypeDefinition();

                if(eventTypeHandlers.TryGetValue(unboundEventType, out SysCol.Dictionary<Type, LinkedList<Delegate>[]> matchingAxes))
                {
                    e.axisInternal.consumed = false;
                    Type raisedAxis = specializedEventType.BaseType.GetGenericArguments()[0];
                    for(Type handledAxis = raisedAxis; handledAxis != typeof(object); handledAxis = handledAxis.BaseType)
                        if (matchingAxes.TryGetValue(handledAxis, out LinkedList<Delegate>[] concreteAxisHandlers))
                            foreach(LinkedList<Delegate> inputLayer in concreteAxisHandlers)
                            {
                                // TODO: support currying further type arguments
                                InputEvent handledEvent = (InputEvent)Activator.CreateInstance(
                                    unboundEventType.MakeGenericType(handledAxis),
                                    [e.axisInternal, e.dataInternal]
                                    );

                                foreach(Delegate handler in inputLayer)
                                    try
                                    {
                                        MeasurementLog.StartMeasure($"Handler '{handler}'");
                                        if ((bool)handler.DynamicInvoke(handledEvent))
                                        {
                                            e.axisInternal.consumed = true;
                                            goto consumed;
                                        }
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

        public LinkedList<InputAxis> GetAxis<DeviceType>(uint axisID)
        {
            LinkedList<InputAxis> axes = new LinkedList<InputAxis>();
            foreach (InputDevice device in EnumerateDevices())
                axes.Add(device[axisID]);

            return axes;
        }

        public float GetValue<DeviceType>(uint axisID)
            => GetAxis<DeviceType>(axisID).Select(InputAxis.SelectValue).Max();

        public bool WasActivated<DeviceType>(uint axisID, float threshold = 0.5f)
            => GetAxis<DeviceType>(axisID).Any(threshold, InputAxis.WasActivated);

        public bool WasReleased<DeviceType>(uint axisID, float threshold = 0.5f)
          => GetAxis<DeviceType>(axisID).Any(threshold, InputAxis.WasReleased);

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
