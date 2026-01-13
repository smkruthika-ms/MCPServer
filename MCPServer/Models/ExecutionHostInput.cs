using System.ComponentModel.DataAnnotations;

namespace MCPServer.Models;

public class ExecutionHostInput
{
    /// <summary>
    /// The main message string.
    /// </summary>
    public string message { get; set; } = string.Empty;

    /// <summary>
    /// Inputs object containing user query and original prompt.
    /// </summary>
    public InputsData inputs { get; set; } = new InputsData();

    public class InputsData
    {
        /// <summary>
        /// The user query - a natural language input that specifies the desired intent.
        /// </summary>
        [Required]
        public string text { get; set; } = string.Empty;

        /// <summary>
        /// This is the original user prompt. Strictly do not change or rephrase the user ask captured.
        /// </summary>
        public string text_3 { get; set; } = string.Empty;
    }
}
