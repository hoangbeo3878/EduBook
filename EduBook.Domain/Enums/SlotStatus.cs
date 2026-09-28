using System;
using System.Collections.Generic;
using System.Text;

namespace EduBook.Domain.Enums
{
    public enum SlotStatus
    {
        Open = 0,     // còn trống, student có thể book
        Booked = 1,   // đã có booking
        Blocked = 2   // tutor khóa, không book được
    }
}
