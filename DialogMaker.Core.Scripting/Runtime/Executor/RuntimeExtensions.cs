namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    public static class RuntimeExtensions
    {
        extension(DSharpStackValueType valueType)
        {
            public bool TryGetBuildInSize(out int result)
            {
                switch (valueType)
                {
                    case DSharpStackValueType.Byte:
                        result = DSharpBuildInTypes.Byte.Size;
                        return true;
                    case DSharpStackValueType.SByte:
                        result = DSharpBuildInTypes.SignedByte.Size;
                        return true;
                    case DSharpStackValueType.Short:
                        result = DSharpBuildInTypes.Short.Size;
                        return true;
                    case DSharpStackValueType.UShort:
                        result = DSharpBuildInTypes.UnsignedShort.Size;
                        return true;
                    case DSharpStackValueType.Int:
                        result = DSharpBuildInTypes.Int.Size;
                        return true;
                    case DSharpStackValueType.UInt:
                        result = DSharpBuildInTypes.UnsignedInt.Size;
                        return true;
                    case DSharpStackValueType.Long:
                        result = DSharpBuildInTypes.Long.Size;
                        return true;
                    case DSharpStackValueType.ULong:
                        result = DSharpBuildInTypes.UnsignedLong.Size;
                        return true;
                    case DSharpStackValueType.Decimal:
                        result = DSharpBuildInTypes.Decimal.Size;
                        return true;
                    case DSharpStackValueType.Double:
                        result = DSharpBuildInTypes.Double.Size;
                        return true;
                    case DSharpStackValueType.Float:
                        result = DSharpBuildInTypes.Single.Size;
                        return true;
                    case DSharpStackValueType.Char:
                        result = DSharpBuildInTypes.Char.Size;
                        return true;
                    case DSharpStackValueType.Bool:
                        result = DSharpBuildInTypes.Boolean.Size;
                        return true;
                    case DSharpStackValueType.Reference:
                        result = DSharpBuildInTypes.NativeInt.Size;
                        return true;
                }

                result = 0;
                return false;
            }
        }
    }
}
