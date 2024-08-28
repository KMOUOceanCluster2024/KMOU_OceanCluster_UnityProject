using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat_Server
{
    public enum PROTOCOL : short
    {
        Setting = 0,

        SIGNUP_Request = 1,
        SIGNUP_Success = 2,
        SIGNUP_Fail = 3,

        LOGIN_Request = 4,
        LOGIN_Success = 5,
        LOGIN_Fail = 6,

        Position_Update = 7,
        Deliver_Position = 8,   //다른 유저의 위치 <- 2단계
        Send_Message = 9,
        Deliver_Message = 10,

        Quest_Start_Request = 11,
        Quest_Start_Success = 12,
        MiniGame_End_Request = 13,
        MiniGame_End_Success = 14,
        Quest_Complete_Request = 15,
        Quest_Complete_Success = 16,

        Delete_User = 17,
        Sub_Quest_End_Request = 18,
        Sub_Quest_End_Success = 19
    }
}
