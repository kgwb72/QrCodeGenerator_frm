using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Globalization;


namespace QrCodeGenerator
{
    public class RangeValidationRule : ValidationRule
    {
        public int Min { get; set; } = 0;
        public int Max { get; set; } = 100;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string stringValue)
            {
                if (int.TryParse(stringValue, out int result))
                {
                    if (result >= Min && result <= Max)
                    {
                        return ValidationResult.ValidResult;
                    }
                }
            }

            return new ValidationResult(false, $"Value must be a number between {Min} and {Max}.");
        }
    }

}
