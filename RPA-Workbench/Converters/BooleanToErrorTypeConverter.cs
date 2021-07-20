//-----------------------------------------------------------------------
// <copyright file="BooleanToErrorTypeConverter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// </copyright>
//-----------------------------------------------------------------------

namespace RPA_Workbench.Converters
{
    using System;
    using System.IO;
    using System.Windows.Data;
    using RPA_Workbench.Properties;

    public class BooleanToErrorTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool? isWarning = value as bool?;
            if (isWarning.Value)
            {
                return Resources.WarningValidationItem;
            }
            else
            {
                return Resources.ErrorValidationItem;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
