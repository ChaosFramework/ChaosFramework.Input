using ChaosFramework.Collections;
using System;
using System.Linq;
using System.Threading;
using MeasurementLog = ChaosUtil.Debug.MeasurementLog;
using SysCol = System.Collections.Generic;

namespace ChaosFramework.Input
{
    using Layouts;

    public class InputContext : Core.Disposable
    {
        internal enum Dummy { Dummy }

        public readonly LayoutManager layoutMgr;

        readonly LinkedList<InputDevice> allDevices = new LinkedList<InputDevice>();
        readonly Thread updateThread;
        readonly AutoResetEvent initEvent = new AutoResetEvent(false);
        readonly AdvancedLinkedList<Delegate>[] inputLayers;

        LinkedList<InputEvent> queuedEvents = new LinkedList<InputEvent>();
        Func<InputContext, InputDeviceHost> getOrCreateHost;
        InputDeviceHost deviceHost;

        SysCol.Dictionary<Delegate, Type> eventTypes = new SysCol.Dictionary<Delegate, Type>();

        public InputContext(Type consumeLayerEnum, Func<InputContext, InputDeviceHost> getOrCreateHost)
        {
            this.getOrCreateHost = getOrCreateHost;
            if (!consumeLayerEnum.IsEnum)
                throw new ArgumentException($"{nameof(consumeLayerEnum)} must be an enum.", nameof(consumeLayerEnum));

            inputLayers = new AdvancedLinkedList<Delegate>[Enum.GetValues(consumeLayerEnum).Length];
            for (int i = 0; i < inputLayers.Length; i++)
                inputLayers[i] = new AdvancedLinkedList<Delegate>();

            layoutMgr = new LayoutManager();
            updateThread = new Thread(InputThread);
            updateThread.Start();
            initEvent.WaitOne();
        }

        public void AddHandler<EventType, AxisType>(Enum layer, Func<EventType, bool> handler)
            where AxisType : InputAxis
            where EventType : InputEvent<AxisType>
        {
            Delegate instance = handler;
            eventTypes[instance] = typeof(EventType);
            inputLayers[(int)(Dummy)layer].Add(instance);
        }

        public void PrependHandler<EventType, AxisType>(Enum layer, Func<EventType, bool> handler)
            where AxisType : InputAxis
            where EventType : InputEvent<AxisType>
        {
            Delegate instance = handler;
            eventTypes[instance] = typeof(EventType);
            inputLayers[(int)(Dummy)layer].Insert(0, instance);
        }

        public void AddHandler(Enum layer, Func<InputEvent, bool> handler)
            => inputLayers[(int)(Dummy)layer].Add(handler);

        public void PrependHandler(Enum layer, Func<InputEvent, bool> handler)
            => inputLayers[(int)(Dummy)layer].Insert(0, handler);

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

        MeasurementLog.CustomAttribute[] NumDevicesCustomAttr(double t)
            => new[] { new MeasurementLog.CustomAttribute("Number of Devices", allDevices.length.ToString()) };

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
                e.axis.consumed = false;
                for (int layer = inputLayers.Length - 1; layer >= 0; layer--)
                {
                    AdvancedLinkedList<Delegate> actions = inputLayers[layer];
                    actions.SetEnumerator(inputLayers[layer].length - 1, -actions.length);
                    foreach (Delegate action in actions)
                    {
                        Type eventType;
                        if (!eventTypes.TryGetValue(action, out eventType) || e.GetType() == eventType)
                            if ((bool)action.DynamicInvoke(e))
                            {
                                e.axis.consumed = true;
                                goto consumed;
                            }
                    }
                }
            consumed:;
            }
            MeasurementLog.EndMeasure();

            foreach (AdvancedLinkedList<Delegate> layer in inputLayers)
                layer.Clear();
            eventTypes.Clear();
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
            foreach (AdvancedLinkedList<Delegate> layer in inputLayers)
                layer.Clear();
            eventTypes.Clear();
        }
    }
}
