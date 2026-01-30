namespace Prog3_WebApi_Javascript.DTOs;

public class GPTRequest
{
    public string model { get; set; }
    public int max_output_tokens { get; set; }
    public double temperature { get; set; }
    public object text { get; set; }
    
    public string? previous_response_id { get; set; }
    public List<Message> input { get; set; } = new List<Message>();

}