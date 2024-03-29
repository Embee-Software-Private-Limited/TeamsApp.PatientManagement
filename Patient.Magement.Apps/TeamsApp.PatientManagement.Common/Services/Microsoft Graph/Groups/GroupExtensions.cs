﻿

namespace TeamsApp.PatientManagement.Bot.Services.MicrosoftGraph
{
    using Microsoft.Graph;
    using System;

    /// <summary>
    /// Group Extension.
    /// </summary>
    public static class GroupExtensions
    {
        /// <summary>
        /// Check if the group's visibility set to hidden membership.
        /// </summary>
        /// <param name="group">Group.</param>
        /// <returns>Indicating if the visibility is hidden membership.</returns>
        public static bool IsHiddenMembership(this Group group)
        {
            var visibility = group.Visibility;
            if (string.IsNullOrWhiteSpace(visibility))
            {
                return false;
            }

            return visibility.Equals(Common.Constants.HiddenMembership, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}