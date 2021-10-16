using System;

namespace MindPlaceApi.Codes
{
    public class AppHelper
    {
        public enum EmploymentType
        {
            FullTime,
            PartTime,
            Contract,
            Internship,
            Freelance,
            SelfEmployed,
            Apprenticeship,
            Seasonal
        }

        public enum FollowStatus
        {//this enum holds the different available status for the follow table.

            //cancelled means the Patient canceled the request.
            CANCELLED,
            //confirmed means there's a mutual relationship between the Patient and the Professional
            CONFIRMED,
            //Pending means the Professional hasn't taking any action on the request yet. The request hasn't been declined or accepted.
            PENDING,
            //Declined means, well, request declined.
            DECLINED
        }

        public enum Roles
        {
            ADMIN,
            MODERATOR,
            PROFESSIONAL, 
            PATIENT
        }

        public enum NotificationType
        {//name is self explanatory.
            COMMENT,
            SUBSCRIPTION
        }
    }
}