namespace RosettaTools.Pwsh.Text.RevenantLogger.Common
{

    public class ValidateTypesAttribute : ValidateArgumentsAttribute
    {
        private readonly Type[] _allowedTypes;

        public ValidateTypesAttribute(params Type[] allowedTypes)
        {
            _allowedTypes = allowedTypes;
        }

        protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
        {
            bool validArgument = false;
            if (arguments == null)
            {
                throw new ValidationMetadataException("Value cannot be null");
            }
            ;
            ;
            ;
            ;
            ;
            //PSObject psObj = arguments as PSObject;
            //if (psObj == null)
            //{
            //    throw new ValidationMetadataException("Value must be a PSObject");
            //}

            //Get the base object's type
            //Type baseType = psObj.BaseObject.GetType();

            foreach (Type type in _allowedTypes)
            {
                //if (type.IsAssignableFrom(arguments.GetType()))
                //{
                //    validArgument = true;
                //    return;
                //}

                if (type.IsAssignableFrom(arguments.GetType()))
                {
                    validArgument = true;
                    return;
                }
                else
                {
                    PSObject? psObj = arguments as PSObject;
                    Type? argType = psObj?.BaseObject.GetType();
                    if (argType != null && type.IsAssignableFrom(argType))
                    {
                        validArgument = true;
                        return;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            throw new ValidationMetadataException(
                    $"Value must be one of the following types: {_allowedTypes.ToString()}.");

            //if (!_allowedTypes.Any(t => t.IsAssignableFrom(baseType)))
            //{
            //    string allowedTypeNames = string.Join(", ", _allowedTypes.Select(t => t.Name));
            //    throw new ValidationMetadataException(
            //        $"Value must be one of the following types: {allowedTypeNames}. Got: {baseType.Name}");
            //}
        }
    }
}
