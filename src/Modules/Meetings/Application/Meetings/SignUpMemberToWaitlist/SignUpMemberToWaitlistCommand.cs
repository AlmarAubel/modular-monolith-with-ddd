﻿using System;
using CompanyName.MyMeetings.BuildingBlocks.Application.Contracts;

namespace CompanyName.MyMeetings.Modules.Meetings.Application.Meetings.SignUpMemberToWaitlist
{
    public class SignUpMemberToWaitlistCommand : CommandBase
    {
        public Guid MeetingId { get; }

        public SignUpMemberToWaitlistCommand(Guid meetingId)
        {
            MeetingId = meetingId;
        }
    }
}