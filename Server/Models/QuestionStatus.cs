using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public enum QuestionStatus
{
    Opened = 1,
    InWork = 2,
    Closed = 3
}