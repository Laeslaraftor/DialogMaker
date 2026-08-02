namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// D# type layout
    /// </summary>
    public readonly struct DSharpTypeLayout(IDSharpType type, int instanceSize, int staticSize, Dictionary<IDSharpFieldInfo, int> instanceFieldOffsets, Dictionary<IDSharpFieldInfo, int> staticFieldOffsets)
    {
        /// <summary>
        /// D# type
        /// </summary>
        public IDSharpType Type { get; } = type;
        /// <summary>
        /// Instance total size. This is sum of instance fields sizes with paddings
        /// </summary>
        public int InstanceSize { get; } = instanceSize;
        /// <summary>
        /// Static total size. This is sum of static fields sizes with paddings
        /// </summary>
        public int StaticSize { get; } = staticSize;
        /// <summary>
        /// Offsets for instance fields with padding relative to 0
        /// </summary>
        public Dictionary<IDSharpFieldInfo, int> InstanceFieldOffsets { get; } = instanceFieldOffsets;
        /// <summary>
        /// Offsets for static fields with padding relative to 0
        /// </summary>
        public Dictionary<IDSharpFieldInfo, int> StaticFieldOffsets { get; } = staticFieldOffsets;

        #region Static

        /// <summary>
        /// Create D# type layout
        /// </summary>
        /// <param name="type">Type for creating layout</param>
        /// <param name="pack">Pack setting (1, 2, 4, 8, 16 or 0 for auto choice)</param>
        /// <returns>D# type layout</returns>
        public static DSharpTypeLayout Create(IDSharpType type, int pack = 0)
        {
            if (pack <= 0)
            {
                pack = int.MaxValue;
            }

            var fields = type.GetAllFields(true).Union(type.GetFields().Where(f => f.IsStatic));
            var instanceFieldOffsets = new Dictionary<IDSharpFieldInfo, int>();
            var staticFieldOffsets = new Dictionary<IDSharpFieldInfo, int>();
            int currentOffset = 0;
            int staticOffset = 0;
            int maxFieldAlignment = 0;
            int structAlignment = 0;

            foreach (var field in fields)
            {
                int fieldSize = GetFieldSize(field, pack);
                int fieldAlignment = GetFieldAlignment(field, pack);

                if (fieldAlignment > structAlignment)
                {
                    structAlignment = fieldAlignment;
                }
            }

            structAlignment = Math.Min(structAlignment, pack);

            foreach (var field in fields)
            {
                int fieldSize = GetFieldSize(field, pack);
                int fieldAlignment = GetFieldAlignment(field, pack);
                int effectiveAlignment = Math.Min(fieldAlignment, pack);

                if (field.IsStatic)
                {
                    staticOffset = Align(staticOffset, effectiveAlignment);
                    staticFieldOffsets[field] = staticOffset;
                    staticOffset += fieldSize;
                }
                else
                {
                    currentOffset = Align(currentOffset, effectiveAlignment);
                    instanceFieldOffsets[field] = currentOffset;
                    currentOffset += fieldSize;

                    if (effectiveAlignment > maxFieldAlignment)
                    {
                        maxFieldAlignment = effectiveAlignment;
                    }
                }
            }

            int instanceSize;

            if (DSharpBuildInTypes.TryGetInfo(type, out var buildInTypeInfo) &&
                buildInTypeInfo.Size != -1)
            {
                instanceSize = buildInTypeInfo.Size;
            }
            else
            {
                instanceSize = Align(currentOffset, structAlignment);
            }

            int staticSize = staticOffset;

            return new(type, instanceSize, staticSize, instanceFieldOffsets, staticFieldOffsets);
        }

        private static int GetFieldSize(IDSharpFieldInfo field, int pack)
        {
            var fieldType = field.FieldType;

            if (fieldType.IsValueType())
            {
                if (DSharpBuildInTypes.TryGetInfo(fieldType, out var typeInfo) &&
                    typeInfo.Size != -1)
                {
                    return typeInfo.Size;
                }

                return Create(fieldType, pack).InstanceSize;
            }

            return DSharpBuildInTypes.NativeInt.Size;
        }

        private static int GetFieldAlignment(IDSharpFieldInfo field, int pack)
        {
            var fieldType = field.FieldType;

            if (fieldType.IsValueType())
            {
                if (DSharpBuildInTypes.TryGetInfo(fieldType, out var typeInfo) &&
                    typeInfo.Size != -1)
                {
                    return typeInfo.Size;
                }

                return GetStructAlignment(fieldType, pack);
            }

            return DSharpBuildInTypes.NativeInt.Size;
        }

        private static int GetStructAlignment(IDSharpType structType, int pack)
        {
            var fields = structType.GetFields();
            int maxAlignment = 0;

            foreach (var field in fields)
            {
                if (field.IsStatic)
                {
                    continue;
                }

                int alignment = GetFieldAlignment(field, pack);

                if (alignment > maxAlignment)
                {
                    maxAlignment = alignment;
                }
            }

            return Math.Min(maxAlignment == 0 ? 1 : maxAlignment, pack);
        }

        private static int Align(int offset, int alignment)
        {
            if (alignment <= 1)
            {
                return offset;
            }

            return (offset + alignment - 1) / alignment * alignment;
        }

        #endregion
    }
}
