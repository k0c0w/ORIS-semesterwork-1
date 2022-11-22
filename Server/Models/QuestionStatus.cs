using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public enum QuestionStatus
{
    Opened,
    InWork,
    Closed
}