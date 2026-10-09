/*
 * Arrakis | Patches/Patchers/GeneralFailMessage.cs
 *
 * Copyright (C) 2026 Arrakis
 * https://github.com/silent-real/Arrakis
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using GorillaNetworking;
using HarmonyLib;
using PlayFab;

namespace Arrakis.Patches
{
    internal static class BanState
    {
        public static bool Captured;
        public static bool Indefinite;
        public static DateTime ExpiryUtc;
        public static string Reason;

        public static void Capture(string reason, string expiry)
        {
            if (!string.IsNullOrEmpty(reason))
                Reason = reason;

            if (string.IsNullOrEmpty(expiry))
                return;

            if (expiry == "Indefinite")
            {
                Captured = true;
                Indefinite = true;
                return;
            }

            if (DateTime.TryParse(expiry, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var utc))
            {
                Captured = true;
                Indefinite = false;
                ExpiryUtc = utc;
            }
        }

        public static void CaptureFromError(PlayFabError error)
        {
            if (error?.ErrorDetails == null)
                return;
            if (error.ErrorMessage == null || !error.ErrorMessage.Contains("banned"))
                return;

            KeyValuePair<string, List<string>> first = error.ErrorDetails.FirstOrDefault();
            if (first.Value != null && first.Value.Count > 0)
                Capture(first.Key, first.Value[0]);
        }
    }

    [HarmonyPatch(typeof(GorillaComputer), "OnErrorShared")]
    public static class CaptureOnErrorShared
    {
        [HarmonyPrefix]
        public static void Prefix(PlayFabError error) =>
            BanState.CaptureFromError(error);
    }

    [HarmonyPatch(typeof(PlayFabAuthenticator), "OnPlayFabError")]
    public static class CaptureOnPlayFabError
    {
        [HarmonyPrefix]
        public static void Prefix(PlayFabError obj) =>
            BanState.CaptureFromError(obj);
    }

    [HarmonyPatch(typeof(PlayFabAuthenticator), "ShowBanMessage")]
    public static class CaptureShowBanMessage
    {
        [HarmonyPrefix]
        public static void Prefix(PlayFabAuthenticator.BanInfo banInfo) =>
            BanState.Capture(banInfo?.BanMessage, banInfo?.BanExpirationTime);
    }

    [HarmonyPatch(typeof(GorillaComputer), nameof(GorillaComputer.GeneralFailureMessage))]
    public static class GeneralFailMessage
    {
        private const string prefix = "Some mad herion addict moderator has banned you.\n\nTheir shitty fucking reason:";
        private static readonly Regex HoursLeft = new Regex(@"HOURS LEFT: (\d+)", RegexOptions.Compiled);
        private static readonly Regex ReasonLine = new Regex(@"REASON: ([^\n]*)", RegexOptions.Compiled);

        [HarmonyPrefix]
        public static void Prefix(ref string failMessage)
        {
            if (string.IsNullOrEmpty(failMessage))
            {
                failMessage = "this game sucks lmao";
                return;
            }

            if (!failMessage.Contains("BANNED"))
                return;

            bool indefinite = failMessage.Contains("INDEFINITELY");
            DateTime? expiryUtc = null;

            if (BanState.Captured)
            {
                indefinite = BanState.Indefinite;
                if (!indefinite)
                    expiryUtc = BanState.ExpiryUtc;
            }
            else
            {
                Match m = HoursLeft.Match(failMessage);
                if (m.Success)
                    expiryUtc = DateTime.UtcNow.AddHours(int.Parse(m.Groups[1].Value) - 1);
            }

            string reason = BanState.Reason;
            if (string.IsNullOrEmpty(reason))
            {
                Match r = ReasonLine.Match(failMessage);
                reason = r.Success ? r.Groups[1].Value.Trim() : "UNKNOWN";
            }

            failMessage = prefix.ToUpper() + "\n " + reason.ToUpperInvariant() + "\n" + FormatUnban(indefinite, expiryUtc);
        }

        private static string FormatUnban(bool indefinite, DateTime? expiryUtc)
        {
            if (indefinite)
                return "UNBANNED WHEN:\n NEVER LMAO";
            if (expiryUtc == null)
                return "UNBANNED WHEN:\n UNKNOWN";

            TimeSpan left = expiryUtc.Value - DateTime.UtcNow;

            if (left < TimeSpan.Zero)
                left = TimeSpan.Zero;

            string when = expiryUtc.Value.ToLocalTime().ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture).ToUpperInvariant();
            return $"UNBANNED WHEN:\n {when}\nTIME LEFT: {(int)left.TotalDays}D {left.Hours}H {left.Minutes}M";
        }
    }
}