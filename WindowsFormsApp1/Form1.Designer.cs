using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using System.Collections;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        //My added parameter
        private Device device = null;
        private bool running = true;
        private ArrayList effectList = new ArrayList();
        private bool button0pressed = false;
        private string joyState = "";

        public bool InitializeInput()
        {
            // Create our joystick device
            foreach (DeviceInstance di in Manager.GetDevices(DeviceClass.GameControl,
                EnumDevicesFlags.AttachedOnly | EnumDevicesFlags.ForceFeeback))
            {
                // Pick the first attached joystick we see
                device = new Device(di.InstanceGuid);
                break;
            }
            if (device == null) // We couldn't find a joystick
                return false;

            device.SetDataFormat(DeviceDataFormat.Joystick);
            device.SetCooperativeLevel(this, CooperativeLevelFlags.Exclusive | CooperativeLevelFlags.Background);
            device.Properties.AxisModeAbsolute = true;
            device.Properties.AutoCenter = false;

            int[] axis = new int[0];
            foreach (DeviceObjectInstance doi in device.Objects)
            {
                if ((doi.Flags & (int)ObjectInstanceFlags.Actuator) != 0)
                {
                    axis = new int[axis.Length + 1];
                    axis[axis.Length - 1] = doi.Offset;
                }
            }

            device.Acquire();

            // Enumerate any axes
            foreach (DeviceObjectInstance doi in device.Objects)
            {
                if ((doi.ObjectId & (int)DeviceObjectTypeFlags.Axis) != 0)
                {
                    // We found an axis, set the range to a max of 10,000
                    device.Properties.SetRange(ParameterHow.ById,
                        doi.ObjectId, new InputRange(-5000, 5000));
                }
            }

            /*
            // Load our feedback file
            EffectList effects = null;
            effects = device.GetEffects(@"C:\MyEffectFile.ffe",
                FileEffectsFlags.ModifyIfNeeded);
            foreach (FileEffect fe in effects)
            {
                EffectObject myEffect = new EffectObject(fe.EffectGuid, fe.EffectStruct,
                    device);
                myEffect.Download();
                effectList.Add(myEffect);
            }
            */


            Effect effect_stop = new Effect();
            effect_stop.SetDirection(new int[axis.Length]);
            effect_stop.SetAxes(new int[axis.Length]);
            effect_stop.ConditionStruct = new Condition[axis.Length];

            effect_stop.Flags = EffectFlags.Cartesian | EffectFlags.ObjectOffsets;
            effect_stop.Duration = int.MaxValue;
            effect_stop.SamplePeriod = 0;
            effect_stop.Gain = 0;
            effect_stop.TriggerButton = (int)Microsoft.DirectX.DirectInput.Button.NoTrigger;
            effect_stop.TriggerRepeatInterval = 0;
            effect_stop.UsesEnvelope = false;
            effect_stop.EffectType = Microsoft.DirectX.DirectInput.EffectType.ConstantForce;
            effect_stop.StartDelay = 0;
            effect_stop.Constant = new Microsoft.DirectX.DirectInput.ConstantForce();
            effect_stop.Constant.Magnitude = 0;




            Effect effect_left = new Effect();
            effect_left.SetDirection(new int[axis.Length]);
            effect_left.SetAxes(new int[axis.Length]);
            effect_left.ConditionStruct = new Condition[axis.Length];

            effect_left.Flags = EffectFlags.Cartesian | EffectFlags.ObjectOffsets;
            effect_left.Duration = int.MaxValue;
            effect_left.SamplePeriod = 0;
            effect_left.Gain = 10000;
            effect_left.TriggerButton = (int)Microsoft.DirectX.DirectInput.Button.NoTrigger;
            effect_left.TriggerRepeatInterval = 0;
            effect_left.UsesEnvelope = false;
            effect_left.EffectType = Microsoft.DirectX.DirectInput.EffectType.ConstantForce;
            effect_left.StartDelay = 0;
            effect_left.Constant = new Microsoft.DirectX.DirectInput.ConstantForce();
            effect_left.Constant.Magnitude = -8500;

            Effect effect_right = new Effect();
            effect_right.SetDirection(new int[axis.Length]);
            effect_right.SetAxes(new int[axis.Length]);
            effect_right.ConditionStruct = new Condition[axis.Length];

            effect_right.Flags = EffectFlags.Cartesian | EffectFlags.ObjectOffsets;
            effect_right.Duration = int.MaxValue;
            effect_right.SamplePeriod = 0;
            effect_right.Gain = 10000;
            effect_right.TriggerButton = (int)Microsoft.DirectX.DirectInput.Button.NoTrigger;
            effect_right.TriggerRepeatInterval = 0;
            effect_right.UsesEnvelope = false;
            effect_right.EffectType = Microsoft.DirectX.DirectInput.EffectType.ConstantForce;
            effect_right.StartDelay = 0;
            effect_right.Constant = new Microsoft.DirectX.DirectInput.ConstantForce();
            effect_right.Constant.Magnitude = 8500;

            EffectObject effectObject_stop= null;
            EffectObject effectObject_left = null;
            EffectObject effectObject_right = null;
            foreach (EffectInformation ei in device.GetEffects(EffectType.ConstantForce))
            {
                effectObject_stop = new EffectObject(ei.EffectGuid, effect_stop, device);
                effectObject_left = new EffectObject(ei.EffectGuid, effect_left, device);
                effectObject_right = new EffectObject(ei.EffectGuid, effect_right, device);

            }

            while(true)
            {
                effectObject_left.SetParameters(effect_left, EffectParameterFlags.Start);
                System.Threading.Thread.Sleep(200);
                effectObject_left.SetParameters(effect_stop, EffectParameterFlags.Start);
                System.Threading.Thread.Sleep(1000);
                effectObject_left.SetParameters(effect_right, EffectParameterFlags.Start);
                System.Threading.Thread.Sleep(200);
                effectObject_left.SetParameters(effect_stop, EffectParameterFlags.Start);
                System.Threading.Thread.Sleep(1000);
            }


            while (running)
            {
                UpdateInputState();
                Application.DoEvents();
            }

            return true;
        }

        private void PlayEffects()
        {
            // See if our effects are playing.
            foreach (EffectObject myEffect in effectList)
            {
                //if (button0pressed == true)
                //{
                //MessageBox.Show("Button Pressed.");
                //  myEffect.Start(1, EffectStartFlags.NoDownload);
                //}

                if (!myEffect.EffectStatus.Playing)
                {
                    // If not, play them
                    myEffect.Start(1, EffectStartFlags.NoDownload);
                }
            }
            //button0pressed = true;
        }

        private void UpdateInputState()
        {
            PlayEffects();

            // Check the joystick state
            JoystickState state = device.CurrentJoystickState;
            device.Poll();
            joyState = "Using JoystickState: \r\n";

            joyState += device.Properties.ProductName;
            joyState += "\n";
            joyState += device.ForceFeedbackState;
            joyState += "\n";
            joyState += state.ToString();

            byte[] buttons = state.GetButtons();
            for (int i = 0; i < buttons.Length; i++)
                joyState += string.Format("Button {0} {1}\r\n", i, buttons[i] != 0 ? "Pressed" : "Not Pressed");

            //label1.Text = joyState;

            //if(buttons[0] != 0)
            //button0pressed = true;

        }


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "Form1";
        }

        #endregion
    }
}

