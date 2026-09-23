using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// SensorId 枚举
/// </summary>
public enum SensorId
{
    Accelerometer = 1,
    Gyroscope = 2,
    AmbientLight = 5,
    MagneticField = 6,
    Barometer = 8,
    Hall = 10,
    Proximity = 12,
    Humidity = 13,
    Orientation = 256,
    Gravity = 257,
    LinearAccelerometer = 258,
    RotationVector = 259,
    AmbientTemperature = 260,
    MagneticFieldUncalibrated = 261,
    GyroscopeUncalibrated = 263,
    SignificantMotion = 264,
    PedometerDetection = 265,
    Pedometer = 266,
    HeartRate = 278,
    WearDetection = 280,
    AccelerometerUncalibrated = 281,
    FusionPressure = 283
}

/// <summary>
/// SensorType 枚举
/// </summary>
public enum SensorType
{
    SensorTypeIdAccelerometer = 1,
    SensorTypeIdGyroscope = 2,
    SensorTypeIdAmbientLight = 5,
    SensorTypeIdMagneticField = 6,
    SensorTypeIdBarometer = 8,
    SensorTypeIdHall = 10,
    SensorTypeIdProximity = 12,
    SensorTypeIdHumidity = 13,
    SensorTypeIdOrientation = 256,
    SensorTypeIdGravity = 257,
    SensorTypeIdLinearAcceleration = 258,
    SensorTypeIdRotationVector = 259,
    SensorTypeIdAmbientTemperature = 260,
    SensorTypeIdMagneticFieldUncalibrated = 261,
    SensorTypeIdGyroscopeUncalibrated = 263,
    SensorTypeIdSignificantMotion = 264,
    SensorTypeIdPedometerDetection = 265,
    SensorTypeIdPedometer = 266,
    SensorTypeIdHeartRate = 278,
    SensorTypeIdWearDetection = 280,
    SensorTypeIdAccelerometerUncalibrated = 281
}

/// <summary>
/// SensorAccuracy 枚举
/// </summary>
public enum SensorAccuracy
{
    AccuracyUnreliable = 0,
    AccuracyLow = 1,
    AccuracyMedium = 2,
    AccuracyHigh = 3
}