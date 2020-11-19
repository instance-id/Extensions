// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/Extensions                    --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------
// -- https://gist.github.com/JohanOtto/add0002bc0bc4d02772629829563a28e     --

using System;
using System.Globalization;
using UnityEngine;

namespace instance.id.Extensions
{
    public static class ConversionExtensions
    {
        #region GetDefaultValue

        /// <summary>
        /// Initialize a value based on a type reference if the value specified is <c>null</c>.
        /// </summary>
        /// <param name="value">An object value to initialize. If it's not null the value is returned.</param>
        /// <param name="type">Type to use as a reference to initialize the value.</param>
        /// <returns>The "initialized" version of the value if it was <c>null</c> or the original value if it was not.</returns>
        public static object GetDefaultValue(this Type type)
        {
            if (type == null)
                return null;
            // Check for integers
            if (type == typeof(int))
                return 0;
            if (type == typeof(string))
                return string.Empty;
            if (type == typeof(bool))
                return false;
            if (type == typeof(float))
                return (float) 0;
            if (type == typeof(double))
                return (double) 0;
            if (type == typeof(decimal))
                return (decimal) 0.0;
            if (type == typeof(DateTime))
                return DateTime.MinValue;
            if (type == typeof(Guid))
                return Guid.Empty;
            if (type == typeof(long))
                return (long) 0;

            if (type.IsEnum)
            {
#if FULL || IPHONE
				foreach (object val in Enum.GetValues(type))
				{
					return val;
				}
#endif
                return null;
            }
            else
            {
                //Debug.Assert(false);
                return null;
            }
        }

        #endregion

        #region Convert

        /// <summary>
        /// Converts the specified source object to the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <returns>The value of the conversion.</returns>
        public static T Convert<T>(this object source)
        {
            return (T) Convert(source, typeof(T));
        }

        #endregion

        #region Convert

        /// <summary>
        /// Converts the specified source object to the specified type.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destinationType">Type of the destination.</param>
        /// <returns></returns>
        public static object Convert(this object source, System.Type destinationType)
        {
            try
            {
                if (source == null)
                {
                    return GetDefaultValue(destinationType);
                }

                if (source.GetType() == destinationType)
                {
                    return source;
                }

                if (destinationType.IsEnum)
                {
                    if (source is int)
                    {
                        return Enum.ToObject(destinationType, System.Convert.ToInt32(source));
                    }

                    if (source is string)
                    {
                        return Enum.Parse(destinationType, (string) source, false);
                    }

                    return Enum.Parse(destinationType, source.ToString(), false);
                }

                if (destinationType == typeof(bool) && source is string)
                {
                    // Note: Do not localize!
                    string value = ((string) source).Trim().ToLower();
                    switch (value)
                    {
                        case "":
                        case "0":
                        case "no":
                        case "off":
                        case "disabled":
                            value = "false";
                            break;
                        case "false":
                            return false;
                        case "1":
                        case "yes":
                        case "on":
                        case "enabled":
                            value = "true";
                            break;
                        case "true":
                            return true;
                    }

                    return bool.Parse(value);
                }

                if (destinationType == typeof(Guid) && source is string)
                {
                    return new Guid((string) source);
                }

                if (destinationType == typeof(string) && source is Guid)
                {
                    return source.ToString();
                }

                if (destinationType == typeof(int) && source is string)
                {
                    int intValue = 0;
                    //float.TryParse(((string)source, out floatValue);
                    int.TryParse((string) source, NumberStyles.Any, null, out intValue);
                    return intValue;
                }

                if (destinationType == typeof(float) && source is string)
                {
#if FULL || IPHONE
					float floatValue = 0f;
					//float.TryParse(((string)source, out floatValue);
					float.TryParse((string)source, NumberStyles.Any,null,out floatValue);
					return floatValue;
#else
                    string srcFloat = (string) source;
                    return string.IsNullOrEmpty(srcFloat) ? (float) 0 : float.Parse(srcFloat);
#endif
                }

                if (destinationType == typeof(double) && source is string)
                {
#if FULL || IPHONE
					double doubleValue = 0;
					//double.TryParse((string)source, out doubleValue);
					double.TryParse((string)source, NumberStyles.Any, null, out doubleValue);
					return doubleValue;
#else
                    string srcFloat = (string) source;
                    return string.IsNullOrEmpty(srcFloat) ? (double) 0 : double.Parse(srcFloat);
#endif
                }

                if (destinationType == typeof(decimal) && source is string)
                {
#if FULL || IPHONE
					decimal decimalValue = 0;
					//decimal.TryParse((string)source, out decimalValue);
					decimal.TryParse((string)source, NumberStyles.Any, null, out decimalValue);
					return decimalValue;
#else
                    string srcFloat = (string) source;
                    return string.IsNullOrEmpty(srcFloat) ? (decimal) 0 : decimal.Parse(srcFloat);
#endif
                }

                if (destinationType == typeof(DateTime) && source is string)
                {
#if FULL || IPHONE
					DateTime dateTime;
					return !DateTime.TryParse((string)source, out dateTime) ? GetDefaultValue(destinationType) : dateTime;
#else
                    string srcFloat = (string) source;
                    return string.IsNullOrEmpty(srcFloat) ? GetDefaultValue(destinationType) : DateTime.Parse(srcFloat);
#endif
                }

                // Finally send to church! lol
                return System.Convert.ChangeType(
                    source,
                    destinationType,
                    CultureInfo.CurrentCulture);
            }
            catch (Exception ex)
            {
                Debug.Log("Issue with conversion. Doing default. Message: " + ex.ToString());
                return GetDefaultValue(destinationType);
            }
        }

        #endregion

        #region ConvertToString

        /// <summary>
        /// Convert a value of any type to a <see cref="System.String"/>
        /// </summary>
        /// <remarks>
        /// Unlike the framework functions, this function provides a generic way of converting any value type to a string. It performs various
        /// other checks to insure that no exceptions will occur on boolean data types for example.
        /// </remarks>
        /// <param name="value">The value to be converted to a string</param>
        /// <exception cref="InvalidCastException"><c>If the conversion was not successfull.</c>.</exception>
        public static string ConvertToString(object value)
        {
            try
            {
                // Check for null values
                if (value == null)
                {
                    return string.Empty;
                }

                Type type = value.GetType();
                // Check for integers
                if (type == typeof(int))
                    return System.Convert.ToString((int) value);
                // Check for doubles
                if (type == typeof(double))
                    return System.Convert.ToString((double) value);
                // Check for booleans
                if (type == typeof(bool))
                    return (bool) value ? "1" : "0";
                // Check for strings
                if (type == typeof(string))
                    return value as string;
                // Check for dates
                if (type == typeof(DateTime))
                {
                    DateTime dateTime = (DateTime) value;
                    return dateTime.ToString();
                }

                return value.ToString();

                // Finally, none of the above... we return whateva
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(string.Format("Error converting value.", ex.Message), ex);
            }
        }

        #endregion


        #region ToTimeString

        /// <summary>
        /// Convert the time span in a time string in the following format: 'hh:mm'.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string ToTimeString(TimeSpan value)
        {
            return value.Hours.ToString().PadLeft(2, '0')
                   + ":" + value.Minutes.ToString().PadLeft(2, '0');
        }

        #endregion

        #region ToTimeSpan

        /// <summary>
        ///  Converts the time string to a time span in the following format: 'hh:mm'.
        /// </summary>
        /// <param name="timeString">The time string.</param>
        /// <returns></returns>
        /// <remarks>
        ///   The string must be in the following format: <c>hh:mm</c>.
        /// </remarks>
        public static TimeSpan ToTimeSpan(string timeString)
        {
            if (string.IsNullOrEmpty(timeString) || timeString.IndexOf(':') == -1)
            {
                return new TimeSpan(0, 0, 0);
            }

            string[] parts = timeString.Split(':');
            if (parts.Length < 2)
                return TimeSpan.Zero;

            int hours = 0;
            int minutes = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                int value;
#if FULL || IPHONE
				if (!int.TryParse(parts[i], out value))
					continue;
#else
                value = int.Parse(parts[i]);
#endif

                switch (i)
                {
                    case 0:
                        hours = value;
                        break;
                    case 1:
                        minutes = value;
                        break;
                }
            }

            if (hours == 0 && minutes == 0)
                return TimeSpan.Zero;

            return new TimeSpan(hours, minutes, 0);
        }

        #endregion
    }
}
