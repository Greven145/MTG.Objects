using System.Runtime.Serialization;

namespace MTG.Object.Generator.Modules.Shared.Exceptions;

[Serializable]
public class CodeGenerationException : Exception {
    public CodeGenerationException(string message) : base(message) {
    }

    protected CodeGenerationException(SerializationInfo info, StreamingContext context) : base(info, context) {
    }
}