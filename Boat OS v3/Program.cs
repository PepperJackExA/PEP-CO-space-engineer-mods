using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // Boat OS V3

        string primaryCockpitName = "Control Seat";
        string podRotorName = "Pod Rotor";



        PID rotorAlignmentPID;

        private enum mode
        {
            Off = 0,
            On = 1
        }

        const double TimeStep = 1.0 / 6.0; // Update10 is 1/6th a second

        mode operatingMode = 0;

        IMyMotorStator podMotor;
        string screenOutput = "";
        List<IMyGyro> gyros = new List<IMyGyro>();
        List<IMyThrust> thrusters = new List<IMyThrust>();
        IMyCockpit cockpit = null;
        int rotation = 0; // 60 / rotation is the thruster direction
        int speed = 0; // four different speeds

        public Program()
        {
            rotorAlignmentPID = new PID(6, 0, 1, TimeStep);

            GridTerminalSystem.GetBlocksOfType<IMyGyro>(gyros, x => x.IsSameConstructAs(Me));

            if (gyros.Count == 0)
            {
                Echo("No gyros found. Please add and recompile");
                return;
            }

            GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters, x => x.IsSameConstructAs(Me));
            if (thrusters.Count == 0)
            {
                Echo("No thrusters found. Please add and recompile");
                return;
            }

            cockpit = GridTerminalSystem.GetBlockWithName(primaryCockpitName) as IMyCockpit;
            if (cockpit == null)
            {
                Echo("No cockpit found. Please add and recompile");
                return;
            }
            cockpit.GetSurface(0).ContentType = ContentType.TEXT_AND_IMAGE;

            podMotor = GridTerminalSystem.GetBlockWithName(podRotorName) as IMyMotorStator;
            if (podMotor == null)
            {
                Echo("No podMotor found. Please add and recompile");
                return;
            }
            podMotor.UpperLimitDeg = 90;
            podMotor.LowerLimitDeg = -90;

            Echo("Setup OK");
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (!string.IsNullOrEmpty(argument))
            {
                string input = argument.ToLower();
                if (input == "off" || input == "stop")
                {
                    operatingMode = mode.Off;
                }
                else if (input == "on" || input == "start")
                {
                    operatingMode = mode.On;
                }
            }


            screenOutput = "";

            Vector3D direction = cockpit.WorldMatrix.Forward; // Simply forward from the ship controller
            double pitch, yaw, roll;
            double cockpitYaw = cockpit.MoveIndicator.X;
            int cockpitForward = (int)cockpit.MoveIndicator.Z;
            int cockpitRoll = (int)cockpit.RollIndicator;

            HelperFunctions.GetRotationAnglesSimultaneous(direction, cockpit.GetNaturalGravity() * -1, cockpit.WorldMatrix, out pitch, out yaw, out roll);
            HelperFunctions.ApplyGyroOverride(pitch, cockpitYaw * 60, roll, gyros, cockpit);

            if (cockpitRoll > 0) rotation += -1;
            else if (cockpitRoll < 0) rotation += 1;

            rotation = MathHelper.Clamp(rotation, -6, 6); //

            double error = rotation * (180 / 12) * (Math.PI / 180) - podMotor.Angle;
            podMotor.TargetVelocityRPM = (float)rotorAlignmentPID.Control(error);

            if (operatingMode > 0) // OS mode on
            {
                if (cockpitForward != 0) speed = MathHelper.Clamp(speed + (-cockpitForward), 0, 4); // because I don't want excessive speed and the forward indicator is negative
                //if 

                thrusters.ForEach(thruster => { thruster.ThrustOverridePercentage = (float)(speed * 0.25); });

                screenOutput += $"OS mode: {operatingMode.ToString()}\n" +
                $"Speed: {speed}\n" +
                $"Cockpit yaw input: {cockpitYaw}\n" +
                $"Cockpit forward input: {cockpitForward}\n" +
                $"Debug: {(int)cockpit.RollIndicator}\n" +
                $"Cockpit roll: {rotation}\n" +
                $"Target: {rotation * (180 / 12)}\n" +
                $"pod Motor error: {error / (Math.PI / 180)}\n" +
                $"Thrusters: {thrusters.Count}\n" +
                $"Gyros: {gyros.Count}\n";

            }
            else // OS mode off
            {
                if (cockpitForward != 0)
                {
                    thrusters.ForEach(thruster => { thruster.ThrustOverridePercentage = -cockpitForward; });
                }
                else
                {
                    thrusters.ForEach(thruster => { thruster.ThrustOverridePercentage = 0; });
                }

                screenOutput += $"OS mode: {operatingMode.ToString()}\n" +
                $"Cockpit yaw input: {cockpitYaw}\n" +
                $"Cockpit forward input: {cockpitForward}\n" +
                $"Thrusters: {thrusters.Count}\n" +
                $"Gyros: {gyros.Count}\n";
            }

            cockpit.GetSurface(0).WriteText(screenOutput);
            Me.GetSurface(0).WriteText(screenOutput);
            Echo(screenOutput);
        }
        public static class HelperFunctions
        {

            // Credit to Whiplash141 for this function
            public static double VectorAngleBetween(Vector3D a, Vector3D b) //returns radians 
            {
                if (Vector3D.IsZero(a) || Vector3D.IsZero(b))
                    return 0;
                else
                    return Math.Acos(MathHelper.Clamp(a.Dot(b) / Math.Sqrt(a.LengthSquared() * b.LengthSquared()), -1, 1));
            }

            //  Credit to Whiplash141 for this function
            public static double DistanceFromVector(Vector3D origin, Vector3D direction, Vector3D currentPos)
            {
                direction.Normalize();
                Vector3D lhs = currentPos - origin;

                double dotP = lhs.Dot(direction);
                Vector3D closestPoint = origin + direction * dotP;
                return Vector3D.Distance(currentPos, closestPoint);
            }

            // Credit to Whiplash141 for this function
            public static Vector3D SafeNormalize(Vector3D a)
            {
                if (Vector3D.IsZero(a))
                    return Vector3D.Zero;

                if (Vector3D.IsUnit(ref a))
                    return a;

                return Vector3D.Normalize(a);
            }

            // Credit to Whiplash141 for this function
            // Credit to Whiplash141 / avaness for this function
            public static void GetRotationAnglesSimultaneous(Vector3D desiredForwardVector, Vector3D desiredUpVector, MatrixD worldMatrix, out double pitch, out double yaw, out double roll)
            {
                desiredForwardVector = SafeNormalize(desiredForwardVector);

                MatrixD transposedWm;
                MatrixD.Transpose(ref worldMatrix, out transposedWm);
                Vector3D.Rotate(ref desiredForwardVector, ref transposedWm, out desiredForwardVector);
                Vector3D.Rotate(ref desiredUpVector, ref transposedWm, out desiredUpVector);

                Vector3D leftVector = Vector3D.Cross(desiredUpVector, desiredForwardVector);
                Vector3D axis;
                double angle;
                if (Vector3D.IsZero(desiredUpVector) || Vector3D.IsZero(leftVector))
                {
                    axis = new Vector3D(desiredForwardVector.Y, -desiredForwardVector.X, 0);
                    angle = Math.Acos(MathHelper.Clamp(-desiredForwardVector.Z, -1.0, 1.0));
                }
                else
                {
                    leftVector = SafeNormalize(leftVector);
                    desiredUpVector = SafeNormalize(desiredUpVector);

                    Vector3D forwardVector = SafeNormalize(Vector3D.Cross(leftVector, desiredUpVector));

                    // Create matrix
                    MatrixD targetMatrix = MatrixD.Zero;
                    targetMatrix.Forward = forwardVector;
                    targetMatrix.Left = leftVector;
                    targetMatrix.Up = desiredUpVector;

                    axis = new Vector3D(targetMatrix.M23 - targetMatrix.M32,
                                        targetMatrix.M31 - targetMatrix.M13,
                                        targetMatrix.M12 - targetMatrix.M21);

                    double trace = targetMatrix.M11 + targetMatrix.M22 + targetMatrix.M33;
                    angle = Math.Acos(MathHelper.Clamp((trace - 1) * 0.5, -1, 1));
                }

                if (Vector3D.IsZero(axis))
                {
                    angle = desiredForwardVector.Z < 0 ? 0 : Math.PI;
                    yaw = angle;
                    pitch = 0;
                    roll = 0;
                    return;
                }

                axis = SafeNormalize(axis);
                // Because gyros rotate about -X -Y -Z, we need to negate our angles
                yaw = -axis.Y * angle;
                pitch = -axis.X * angle;
                roll = -axis.Z * angle;
            }

            // Credit to Whiplash141 for this function
            public static void ApplyGyroOverride(double pitch_speed, double yaw_speed, double roll_speed, List<IMyGyro> gyro_list, IMyTerminalBlock reference)
            {
                var rotationVec = new Vector3D(pitch_speed, yaw_speed, roll_speed); //because keen does some weird stuff with signs
                var shipMatrix = reference.WorldMatrix;
                var relativeRotationVec = Vector3D.TransformNormal(rotationVec, shipMatrix);
                foreach (var thisGyro in gyro_list)
                {
                    var gyroMatrix = thisGyro.WorldMatrix;
                    var transformedRotationVec = Vector3D.TransformNormal(relativeRotationVec, Matrix.Transpose(gyroMatrix));
                    thisGyro.Pitch = (float)transformedRotationVec.X;
                    thisGyro.Yaw = (float)transformedRotationVec.Y;
                    thisGyro.Roll = (float)transformedRotationVec.Z;
                    thisGyro.GyroOverride = true;
                }
            }
        }

        #region PID Class

        /// <summary>
        /// Discrete time PID controller class.
        /// Last edited: 2022/08/11 - Whiplash141
        /// </summary>
        public class PID
        {
            public double Kp { get; set; } = 0;
            public double Ki { get; set; } = 0;
            public double Kd { get; set; } = 0;
            public double Value { get; private set; }

            double _timeStep = 0;
            double _inverseTimeStep = 0;
            double _errorSum = 0;
            double _lastError = 0;
            bool _firstRun = true;

            public PID(double kp, double ki, double kd, double timeStep)
            {
                Kp = kp;
                Ki = ki;
                Kd = kd;
                _timeStep = timeStep;
                _inverseTimeStep = 1 / _timeStep;
            }

            protected virtual double GetIntegral(double currentError, double errorSum, double timeStep)
            {
                return errorSum + currentError * timeStep;
            }

            public double Control(double error)
            {
                //Compute derivative term
                double errorDerivative = (error - _lastError) * _inverseTimeStep;

                if (_firstRun)
                {
                    errorDerivative = 0;
                    _firstRun = false;
                }

                //Get error sum
                _errorSum = GetIntegral(error, _errorSum, _timeStep);

                //Store this error as last error
                _lastError = error;

                //Construct output
                Value = Kp * error + Ki * _errorSum + Kd * errorDerivative;
                return Value;
            }

            public double Control(double error, double timeStep)
            {
                if (timeStep != _timeStep)
                {
                    _timeStep = timeStep;
                    _inverseTimeStep = 1 / _timeStep;
                }
                return Control(error);
            }

            public virtual void Reset()
            {
                _errorSum = 0;
                _lastError = 0;
                _firstRun = true;
            }
        }

        public class DecayingIntegralPID : PID
        {
            public double IntegralDecayRatio { get; set; }

            public DecayingIntegralPID(double kp, double ki, double kd, double timeStep, double decayRatio) : base(kp, ki, kd, timeStep)
            {
                IntegralDecayRatio = decayRatio;
            }

            protected override double GetIntegral(double currentError, double errorSum, double timeStep)
            {
                return errorSum * (1.0 - IntegralDecayRatio) + currentError * timeStep;
            }
        }

        public class ClampedIntegralPID : PID
        {
            public double IntegralUpperBound { get; set; }
            public double IntegralLowerBound { get; set; }

            public ClampedIntegralPID(double kp, double ki, double kd, double timeStep, double lowerBound, double upperBound) : base(kp, ki, kd, timeStep)
            {
                IntegralUpperBound = upperBound;
                IntegralLowerBound = lowerBound;
            }

            protected override double GetIntegral(double currentError, double errorSum, double timeStep)
            {
                errorSum = errorSum + currentError * timeStep;
                return Math.Min(IntegralUpperBound, Math.Max(errorSum, IntegralLowerBound));
            }
        }

        public class BufferedIntegralPID : PID
        {
            readonly Queue<double> _integralBuffer = new Queue<double>();
            public int IntegralBufferSize { get; set; } = 0;

            public BufferedIntegralPID(double kp, double ki, double kd, double timeStep, int bufferSize) : base(kp, ki, kd, timeStep)
            {
                IntegralBufferSize = bufferSize;
            }

            protected override double GetIntegral(double currentError, double errorSum, double timeStep)
            {
                if (_integralBuffer.Count == IntegralBufferSize)
                    _integralBuffer.Dequeue();
                _integralBuffer.Enqueue(currentError * timeStep);
                return _integralBuffer.Sum();
            }

            public override void Reset()
            {
                base.Reset();
                _integralBuffer.Clear();
            }
        }

        #endregion

        // THe end
    }
}
