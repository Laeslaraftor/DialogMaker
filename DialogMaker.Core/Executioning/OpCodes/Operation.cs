using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DialogMaker.Core.Executioning.OpCodes
{
    public struct Operation
    {
        public Operation(DialogByteCode code)
        {
            Code = code;
            Arguments = new int[CodeArguments[code]];
        }

        public DialogByteCode Code { get; }
        public int[] Arguments { get; }

        #region Статика

        private static ReadOnlyDictionary<DialogByteCode, int> CodeArguments
        {
            get
            {
                if (field == null)
                {
                    Dictionary<DialogByteCode, int> args = [];

                    foreach (var value in Enum.GetValues(typeof(DialogByteCode)))
                    {
                        var argsAttribute = value.GetEnumAttribute<ArgsCountAttribute>();
                        int count = 0;

                        if (argsAttribute != null)
                        {
                            count = (int)argsAttribute.ArgumentsCount;
                        }

                        args.Add((DialogByteCode)value, count);
                    }

                    field = new(args);
                }

                return field;
            }
        }

        #endregion
    }
}
