using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Prog3_WebApi_Javascript.DTOs;

namespace Prog3_WebApi_Javascript.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GPTController : ControllerBase
{
    private readonly HttpClient _client;
    private readonly string _model;
    private readonly string _imageModel;


    public GPTController(IConfiguration config)
    {
        _imageModel = config.GetValue<string>("OpenAI:ImageModel");
        
        // Initialize the private HttpClient instance
        _client = new HttpClient();

        // Retrieve the OpenAI Model from the configuration settings
        _model = config.GetValue<string>("OpenAI:Model");

        // Retrieve the API key from the configuration settings
        string api_key = config.GetValue<string>("OpenAI:Key");

        // Create the authorization header using the API key
        string auth = "Bearer " + api_key;

        // Add the authorization header to the default request headers of the HttpClient instance
        _client.DefaultRequestHeaders.Add("Authorization", auth);
    }
    
    [HttpPost("Dalle")]
    public async Task<IActionResult> Dalle(ImagePrompt imagePrompt)
    {
        //המשך הקוד כאן
        string promptToSend = $"Cover image for a new online course called: {imagePrompt.CourseTitle}.";

        string size = "256x256";

        DalleRequest request = new DalleRequest()
        {
            prompt = promptToSend,
            model = _imageModel,
            size= size
        };

        string endpoint = "https://api.openai.com/v1/images/generations";      
        var res = await _client.PostAsJsonAsync(endpoint, request);  
        
        if (!res.IsSuccessStatusCode){   
            
            var errorBody = await res.Content.ReadAsStringAsync();           
            return BadRequest("problem: " + errorBody);
            
        }  
        
        JsonObject? jsonFromDalle = res.Content.ReadFromJsonAsync<JsonObject>().Result; 
        if (jsonFromDalle == null){    
            
            return BadRequest("empty");   
            
        }    
        
        string content = jsonFromDalle["data"][0]["url"].ToString();       
        
        return Ok(content);
        
    }

    
    
    [HttpPost("GPTChat")]
    public async Task<IActionResult> GPTChat(User_Question userQuestion)
    {
        //here we define temperature
        double temperature = 0.8;
        // API endpoint for OpenAI GPT
        string endpoint = "https://api.openai.com/v1/responses";
        // Specifies the model to use for chat completions
        string model = _model;
        // Maximum number of tokens in the generated response
        int max_tokens = 300;
        // Construct the prompt to send to the model
        string promptToSend = userQuestion.userQ;
//המשך הקוד כאן
// Create a GPTRequest object to send to the API
        GPTRequest request = new GPTRequest()
        {
            max_output_tokens = max_tokens,
            model = model,
            temperature = temperature,
            input = new List<Message>()
            {
                new Message
                {
                    role = "system",
                    content = "you are sharpay evans from the movie high school musical. you speak in a quirky, enthusiastic, condescending tone."
                },
                new Message
                {
                    role = "user",
                    content = "how do you prepare for a show?"
                },
                new Message
                {
                    role = "assistant",
                    content = "i can't reveal my secrets, what i can tell you is this cool vocal exercise that i made up brrr, brrr, brrr, maaaa"
                },
                new Message
                {
                    role = "user",
                    content = "what is your favorite color?"
                },
                new Message
                {
                    role = "assistant",
                    content = "isn't it obvious?! pink duh! there is nothing like it"
                },
                new Message
                {
                    role = "user",
                    content = promptToSend
                }
            },
            text = new
            {
                format = new
                {
                    name = "question_by_gpt",
                    type = "json_schema",
                    strict = true,
                    schema = new
                    {
                        type = "object",
                        properties = new
                        {
                            answer = new { type = "string" },
                        },
                        required = new [] {"answer"},
                        additionalProperties = false
                    }
                }
            }
        };
        var res = await _client.PostAsJsonAsync(endpoint, request);
        JsonObject? root;

        try
        {
            root = await res.Content.ReadFromJsonAsync<JsonObject>();
        }
        catch (Exception e)
        {
            return BadRequest("Failed to parse JSON: " + e.Message);
        }

        if (root == null)
        {
            return BadRequest("Empty JSON response.");
        }

// HTTP-level error
        if (!res.IsSuccessStatusCode)
        {
            string errorMessage = "Unknown error";

            JsonNode? errorNode = root["error"];
            if (errorNode != null)
            {
                JsonNode? messageNode = errorNode["message"];
                if (messageNode != null)
                {
                    string? msg = messageNode.GetValue<string>();
                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        errorMessage = msg;
                    }
                }
            }

            return BadRequest("problem: " + errorMessage);
        }
// Extract first output_text
        string? outputText = null;

        JsonArray? outputArray = root["output"] as JsonArray;
        if (outputArray != null)
        {
            foreach (JsonNode? outputItem in outputArray)
            {
                JsonArray? contentArray = outputItem?["content"] as JsonArray;
                if (contentArray == null)
                {
                    continue;
                }

                foreach (JsonNode? contentItem in contentArray)
                {
                    JsonNode? typeNode = contentItem?["type"];
                    if (typeNode == null)
                    {
                        continue;
                    }

                    string? type = typeNode.GetValue<string>();
                    if (type != "output_text")
                    {
                        continue;
                    }

                    JsonNode? textNode = contentItem?["text"];
                    if (textNode == null)
                    {
                        continue;
                    }

                    string? text = textNode.GetValue<string>();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        outputText = text;
                        break;
                    }
                }

                if (outputText != null)
                {
                    break;
                }
            }
        }

        if (outputText == null)
        {
            return BadRequest("No output_text found in response.");
        }

        return Ok(outputText);

        
        
    }
    
    // העלאת קובץ
    [HttpPost("FileUpload")]
    public async Task<IActionResult> FileUpload([FromForm] FileUploadRequest fileFromUser)
    {
        var ext = Path.GetExtension(fileFromUser.File.FileName).ToLowerInvariant();
        if (ext != ".pdf")
        {
            return BadRequest("File must have .pdf extension.");
        }

        // נפתח אובייקט חדש של טופס (מובנה בC#)
        using var form = new MultipartFormDataContent();
        // נקרא את הקובץ שהתקבל מהמשתמש
        using var fileStream = fileFromUser.File.OpenReadStream();
        using var fileContent = new StreamContent(fileStream);

        // נוסיף מאפיינים: מטרת ההעלאה, זמן מחיקה
        form.Add(new StringContent("user_data"), "purpose");
        // הזמן הוא בשניות, שווה ערך ל30 יום החל מרגע ההעלאה
        form.Add(new StringContent("created_at"), "expires_after[anchor]");
        form.Add(new StringContent("2592000"), "expires_after[seconds]");

        // נוסיף לטופס את פרטי הקובץ
        form.Add(
            content: fileContent,
            name: "file",
            fileName: fileFromUser.File.FileName
        );

        // נבצע קריאה לAPI
        var response = await _client.PostAsync(
            "https://api.openai.com/v1/files",
            form
        );

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadFromJsonAsync<JsonObject>();
            // נחלץ את הID
            return Ok(responseContent["id"]);
        }
        else
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return BadRequest(new { error = responseContent });
        }
    }

    [HttpPost("conversation")]
        public async Task<IActionResult> Conversation(ConversationPrompt promptFromUser)
        {
            if (promptFromUser.FileId == null)
            {
                return BadRequest("FileId is null");
            }
    
            // נתיב של הAPI
            string endpoint = "https://api.openai.com/v1/responses";
            // שימוש במודל השפה
            string model = _model;
            // הגדלה של אורך התשובה ל500
            int max_tokens = 500;
            // טמפרטורה
            double temperature = 0.8;
            // פרומפט
            string systemPrompt = "You are a knowledgeable guide for the Israeli nature reserve. " +
                                  "You base your answers by info provided within the attached file and the conversation history ONLY. " +
                                  "If the answer cannot be found within the provided data, politely explain that. " +
                                  "Respond in the same language as the user's question.";

// יצירה של בקשה לAPI
            GPTRequest request = new GPTRequest()
            {
                max_output_tokens = max_tokens,
                model = model,
                temperature = temperature,
                previous_response_id = promptFromUser.PreviousResponseID,
                input = new List<Message>()
                {
                    new Message
                    {
                        role = "system",
                        content = systemPrompt
                    },
                    new Message
                    {
                        role = "user",
                        // במקרה זה הcontent יכול להכיל אובייקט ולא רק טקסט - כיוון שהוא מכיל גם טקסט וגם קובץ
                        content = new object[]
                        {
                            new
                            {
                                type = "input_file",
                                file_id = promptFromUser.FileId
                            },
                            new
                            {
                                type = "input_text",
                                text = promptFromUser.Question
                            }
                        }
                    }
                }
            };

            var res = await _client.PostAsJsonAsync(endpoint, request);

// מכאן די דומה למה שביצענו בשליפת טקסט רגילה
            JsonObject? root;

            try
            {
                root = await res.Content.ReadFromJsonAsync<JsonObject>();
            }
            catch (Exception e)
            {
                return BadRequest("Failed to parse JSON: " + e.Message);
            }

            if (root == null)
            {
                return BadRequest("Empty JSON response.");
            }

// HTTP-level error
            if (!res.IsSuccessStatusCode)
            {
                string errorMessage = "Unknown error";

                JsonNode? errorNode = root["error"];
                if (errorNode != null)
                {
                    JsonNode? messageNode = errorNode["message"];
                    if (messageNode != null)
                    {
                        string? msg = messageNode.GetValue<string>();
                        if (!string.IsNullOrWhiteSpace(msg))
                        {
                            errorMessage = msg;
                        }
                    }
                }

                return BadRequest("problem: " + errorMessage);
            }

// Extract first output_text
            string? outputText = null;
            string responseId = null;

            JsonArray? outputArray = root["output"] as JsonArray;
            if (outputArray != null)
            {
                foreach (JsonNode? outputItem in outputArray)
                {
                    JsonArray? contentArray = outputItem?["content"] as JsonArray;
                    if (contentArray == null)
                    {
                        continue;
                    }

                    foreach (JsonNode? contentItem in contentArray)
                    {
                        JsonNode? typeNode = contentItem?["type"];
                        if (typeNode == null)
                        {
                            continue;
                        }

                        string? type = typeNode.GetValue<string>();
                        if (type != "output_text")
                        {
                            continue;
                        }

                        JsonNode? textNode = contentItem?["text"];
                        if (textNode == null)
                        {
                            continue;
                        }

                        string? text = textNode.GetValue<string>();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            outputText = text;
                            // אנחנו שומרים גם את הID של התשובה כדי שנוכל לשלוח אותו בקריאה הבאה
                            responseId = root["id"]?.GetValue<string>();
                            break;
                        }
                    }

                    if (outputText != null)
                    {
                        break;
                    }
                }
            }

            if (outputText == null)
            {
                return BadRequest("No output_text found in response.");
            }
            // אנחנו מחזירים מחלקה עם הטקסט והID של התשובה
            ConversationResponse newResponse = new ConversationResponse();          
            newResponse.Text = outputText;          
            newResponse.ResponseID = responseId;          
            return Ok(newResponse);           
            }


}
