using System.ComponentModel.DataAnnotations;

namespace deepdiveapi.Entities.Validation
{
    /// <summary>
    /// Specifies that the annotated date or time property must be later than another named property on the same object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class LaterThanAttribute : ValidationAttribute
    {
        private readonly string _earlierPropertyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="LaterThanAttribute"/> class.
        /// </summary>
        /// <param name="earlierPropertyName">The name of the property to compare with.</param>
        public LaterThanAttribute(string earlierPropertyName)
        {
            _earlierPropertyName = earlierPropertyName;
        }

        /// <summary>
        /// Validates that the value of the property is later than the value of the referenced property.
        /// </summary>
        /// <param name="value">The value of the property being validated.</param>
        /// <param name="validationContext">Describes the context in which a validation check is performed.</param>
        /// <returns>A <see cref="ValidationResult"/> that represents the success or failure of the validation.</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var earlierPropertyInfo = validationContext.ObjectType.GetProperty(_earlierPropertyName);

            if (earlierPropertyInfo == null)
            {
                return new ValidationResult($"Property {_earlierPropertyName} not found");
            }

            var earlierValue = earlierPropertyInfo.GetValue(validationContext.ObjectInstance, null) as IComparable;
            var laterValue = value as IComparable;

            if (earlierValue != null && laterValue != null && laterValue.CompareTo(earlierValue) <= 0)
            {
                return new ValidationResult($"{validationContext.DisplayName} must be later than {_earlierPropertyName}");
            }

            return ValidationResult.Success;
        }
    }

}
