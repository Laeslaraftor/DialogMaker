namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Parameter information
    /// </summary>
    /// <param name="name">Name of parameter</param>
    /// <param name="type">Type of parameter</param>
    /// <param name="mode">Parameter mode</param>
    public readonly struct DSharpParameterInfo(string name, IDSharpType type, DSharpMethodParameterMode mode) 
        : IDSharpParameterInfo, IEquatable<DSharpParameterInfo>, IEquatable<IDSharpParameterInfo>
    {
        public string Name { get; } = name;
        public IDSharpType Type { get; } = type;
        public DSharpMethodParameterMode Mode { get; } = mode;

        #region Controls

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Type, Mode);
        }
        public override bool Equals(object obj)
        {
            return (obj is DSharpParameterInfo other && Equals(other)) ||
                   (obj is IDSharpParameterInfo other2 && Equals(other2));
        }
        public bool Equals(DSharpParameterInfo other)
        {
            return Name == other.Name &&
                   Type == other.Type &&
                   Mode == other.Mode;
        }
        public bool Equals(IDSharpParameterInfo other)
        {
            return Name == other.Name &&
                   Type == other.Type &&
                   Mode == other.Mode;
        }

        public override string ToString()
        {
            return $"{Mode.ToString().ToLower()} {Type} {(string.IsNullOrEmpty(Name) ? string.Empty : Name)}";
        }

        #endregion

        #region Operators

        public static bool operator ==(DSharpParameterInfo l, DSharpParameterInfo r) => l.Equals(r);
        public static bool operator !=(DSharpParameterInfo l, DSharpParameterInfo r) => !l.Equals(r);
        public static bool operator ==(IDSharpParameterInfo l, DSharpParameterInfo r) => r.Equals(l);
        public static bool operator !=(IDSharpParameterInfo l, DSharpParameterInfo r) => !r.Equals(l);
        public static bool operator ==(DSharpParameterInfo l, IDSharpParameterInfo r) => l.Equals(r);
        public static bool operator !=(DSharpParameterInfo l, IDSharpParameterInfo r) => !l.Equals(r);

        #endregion

        #region Static

        /// <summary>
        /// Write parameter information to stream
        /// </summary>
        /// <param name="stream">Stream to writing parameter information</param>
        /// <param name="parameter">Parameter that will be wrote</param>
        /// <param name="writeName">Should write parameter name</param>
        public static void Write(Stream stream, IDSharpParameterInfo parameter, bool writeName = true)
        {
            string name = parameter.Name;

            if (!writeName)
            {
                name = string.Empty;
            }

            stream.WriteString(name);
            parameter.Type.MetadataToken.Write(stream);
            stream.WriteByte((byte)parameter.Mode);
        }
        /// <summary>
        /// Read parameter information from stream
        /// </summary>
        /// <param name="stream">Stream for reading</param>
        /// <param name="assembly">Assembly for finding parameter type by metadata token</param>
        /// <returns>Parameter information from stream</returns>
        public DSharpParameterInfo Read(Stream stream, IDSharpAssembly assembly)
        {
            var name = stream.ReadString();
            var typeToken = DSharpMetadataToken.Read(stream);
            var mode = stream.Read<DSharpMethodParameterMode>();
            
            if (assembly.GetType(typeToken) is not IDSharpType type)
            {
                throw new InvalidOperationException($"Unable to get type for metadata token: {typeToken}");
            }

            return new(name, type, mode);
        }

        #endregion
    }
}
