/*
 * Arrakis | Menu/Buttons.cs
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

using Arrakis.Classes;
using Arrakis.Managers;
using Arrakis.Mods;
using Arrakis.Notifications;
using Arrakis.Patches;
using Arrakis.Patches.Patchers;
using GorillaTagScripts;
using Photon.Pun;
using UnityEngine;
using static Arrakis.Menu.Main;
using static Arrakis.Settings;

namespace Arrakis.Menu
{
    public class Buttons
    {
        public static ButtonInfo[][] buttons = new ButtonInfo[][]
        {
            new ButtonInfo[] { // Main
                new ButtonInfo { buttonText = "Discord", method =() => JoinDiscord(), isTogglable = false, toolTip = "Prompts you if you want to join the discord or not." },
                new ButtonInfo { buttonText = "Settings", method =() => CurrentCategoryName = "Settings", isTogglable = false, toolTip = "Opens the settings page for the menu." },

                new ButtonInfo { buttonText = "Players", method =() => Players(), isTogglable = false, toolTip = "Opens the players page for the menu." },

                new ButtonInfo { buttonText = "Enabled", method =() => CurrentCategoryName = "Enabled", isTogglable = false, toolTip = "Opens the enabled page for the menu." },
                new ButtonInfo { buttonText = "Favorites", method =() => CurrentCategoryName = "Favorites", isTogglable = false, toolTip = "Opens the favorites page for the menu." },

                new ButtonInfo { buttonText = "Rooms", method =() => LoadRooms(), isTogglable = false, toolTip = "Opens the rooms page for the menu." },
                new ButtonInfo { buttonText = "Global", method =() => CurrentCategoryName = "Global", isTogglable = false, toolTip = "Opens the global page for the menu." },

                new ButtonInfo { buttonText = "Important", method =() => CurrentCategoryName = "Important", isTogglable = false, toolTip = "Opens the important page for the menu." },
                new ButtonInfo { buttonText = "Computer", method =() => CurrentCategoryName = "Computer", isTogglable = false, toolTip = "Opens the computer page for the menu." },
                new ButtonInfo { buttonText = "Safety", method =() => CurrentCategoryName = "Safety", isTogglable = false, toolTip = "Opens the safety page for the menu." },
                new ButtonInfo { buttonText = "Movement", method =() => CurrentCategoryName = "Movement", isTogglable = false, toolTip = "Opens the movement page for the menu." },
                new ButtonInfo { buttonText = "VRRig", method =() => CurrentCategoryName = "VRRig", isTogglable = false, toolTip = "Opens the vrrig page for the menu." },
                new ButtonInfo { buttonText = "Visual", method =() => CurrentCategoryName = "Visual", isTogglable = false, toolTip = "Opens the visual page for the menu." },
                new ButtonInfo { buttonText = "Advantage", method =() => CurrentCategoryName = "Advantage", isTogglable = false, toolTip = "Opens the advantage page for the menu." },
                new ButtonInfo { buttonText = "Sound", method =() => CurrentCategoryName = "Sound", isTogglable = false, toolTip = "Opens the sound page for the menu." },
                new ButtonInfo { buttonText = "Fun", method =() => CurrentCategoryName = "Fun", isTogglable = false, toolTip = "Opens the fun page for the menu." },
                new ButtonInfo { buttonText = "Soundboard", method =() => Soundboard.StartSoundboard(), isTogglable = false, toolTip = "Opens the soundboard page for the menu." },
                new ButtonInfo { buttonText = "Projectiles", method =() => CurrentCategoryName = "Projectiles", isTogglable = false, toolTip = "Opens the projectiles page for the menu." },
                new ButtonInfo { buttonText = "Overpowered", method =() => CurrentCategoryName = "Overpowered", isTogglable = false, toolTip = "Opens the overpowered page for the menu." },
                new ButtonInfo { buttonText = "Custom Maps", method =() => CurrentCategoryName = "Custom Maps", isTogglable = false, toolTip = "Opens the custom maps page for the menu." },
                new ButtonInfo { buttonText = "Master", method =() => CurrentCategoryName = "Master", isTogglable = false, toolTip = "Opens the master page for the menu." },
                new ButtonInfo { buttonText = "Experimental", method =() => CurrentCategoryName = "Experimental", isTogglable = false, toolTip = "Opens the experimental page for the menu." },
            },

            new ButtonInfo[] { // Settings
                new ButtonInfo { buttonText = "Exit Settings", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page of the menu." },

                new ButtonInfo { buttonText = "Menu Settings", method =() => CurrentCategoryName = "Menu Settings", isTogglable = false, toolTip = "Opens the settings for the menu." },
                new ButtonInfo { buttonText = "Movement Settings", method =() => CurrentCategoryName = "Movement Settings", isTogglable = false, toolTip = "Opens the movement settings for the menu." },
                new ButtonInfo { buttonText = "Visual Settings", method =() => CurrentCategoryName = "Visual Settings", isTogglable = false, toolTip = "Opens the visual settings for the menu." },
                new ButtonInfo { buttonText = "Advantage Settings", method =() => CurrentCategoryName = "Advantage Settings", isTogglable = false, toolTip = "Opens the advantage settings for the menu." },
                new ButtonInfo { buttonText = "Projectile Settings", method =() => CurrentCategoryName = "Projectile Settings", isTogglable = false, toolTip = "Opens the projectile settings for the menu." },
                new ButtonInfo { buttonText = "Gunlib Settings", method =() => CurrentCategoryName = "Gunlib Settings", isTogglable = false, toolTip = "Opens the gunlib settings for the menu." },
                new ButtonInfo { buttonText = "Safety Settings", method =() => CurrentCategoryName = "Safety Settings", isTogglable = false, toolTip = "Opens the safety settings for the menu." },
                new ButtonInfo { buttonText = "Plugin Settings", method =() => CurrentCategoryName = "Plugin Settings", isTogglable = false, toolTip = "Opens the plugin settings for the menu." },
            },

            new ButtonInfo[] { // Menu Settings
                new ButtonInfo { buttonText = "Exit Menu Settings", method =() => CurrentCategoryName = "Settings", isTogglable = false, toolTip = "Returns to the main settings page for the menu." },

                new ButtonInfo { buttonText = "Right Hand", enableMethod =() => rightHanded = true, disableMethod =() => rightHanded = false, toolTip = "Puts the menu on your right hand.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Notifications", enableMethod =() => disableNotifications = false, disableMethod =() => disableNotifications = true, enabled = !disableNotifications, toolTip = "Toggles the notifications.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "FPS Counter", enableMethod =() => fpsCounter = true, disableMethod =() => fpsCounter = false, enabled = fpsCounter, toolTip = "Toggles the FPS counter.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disconnect Button", enableMethod =() => disconnectButton = true, disableMethod =() => disconnectButton = false, enabled = disconnectButton, toolTip = "Toggles the disconnect button.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Change Menu Theme", overlapText = "Change Menu Theme <color=grey>[<color=cyan>Default</color>]</color>", enableMethod =() => ChangeMenuTheme(), method =() => ChangeMenuTheme(), disableMethod =() => ChangeMenuTheme(false), isIncremental = true, isTogglable = false, toolTip = "Changes the menu theme.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Outline Menu", enableMethod =() => outlineMenu = true, disableMethod =() => outlineMenu = false, isTogglable = true, toolTip = "Gives the menu a outline.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Change Click Sound", overlapText = "Change Click Sound <color=grey>[<color=cyan>buttonpress</color>]</color>", enableMethod =() => AudioManager.ChangeClickSound(), method =() => AudioManager.ChangeClickSound(), disableMethod =() => AudioManager.ChangeClickSound(false), isIncremental = true, isTogglable = false, toolTip = "Changes the click sound of the menu buttons.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Change Click Volume", overlapText = "Change Click Volume <color=grey>[<color=cyan>0.4f</color>]</color>", enableMethod =() => ChangeButtonClickVolume(), method =() => ChangeButtonClickVolume(), disableMethod =() => ChangeButtonClickVolume(false), isIncremental = true, isTogglable = false, toolTip = "Changes the click sound volume of the menu buttons.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Freeze Player In Menu", method =() => FreezePlayerInMenu(), isTogglable = true, toolTip = "Freezes you in the air when you have the menu open.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Freeze Rig In Menu", method =() => FreezeRigInMenu(), disableMethod =() => Movement.FixRig(), isTogglable = true, toolTip = "Freezes your rig when you have the menu open.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Change Font Style", overlapText = "Change Font Style <color=grey>[<color=cyan>Default</color>]</color>", enableMethod =() => ChangeFontStyle(), method =() => ChangeFontStyle(), disableMethod =() => ChangeFontStyle(false), isIncremental = true, isTogglable = false, toolTip = "Changes the font style of the menu text.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Change Font", overlapText = "Change Font <color=grey>[<color=cyan>Default</color>]</color>", enableMethod =() => ChangeFont(), method =() => ChangeFont(), disableMethod =() => ChangeFont(false), isIncremental = true, isTogglable = false, toolTip = "Changes the font of the menu.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Change Notification Delay", overlapText = "Change Notification Delay <color=grey>[<color=cyan>Default</color>]</color>", enableMethod =() => ChangeNotificationDelay(), method =() => ChangeNotificationDelay(), disableMethod =() => ChangeNotificationDelay(false), isIncremental = true, isTogglable = false, toolTip = "Changes the notification delay.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Menu Sounds", method =() => menusounds = false, disableMethod =() => menusounds = true, isTogglable = true, toolTip = "Disables the menu sounds.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Click Sounds", method =() => disablebuttonsounds = true, disableMethod =() => disablebuttonsounds = false, isTogglable = true, toolTip = "Disables the button clicks you get when you press a button.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Click Vibrations", method =() => disablevibrations = true, disableMethod =() => disablevibrations = false, isTogglable = true, toolTip = "Disables the contorller vibrations you get when you press a button.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Utils GUI", method =() => utilsgui = false, disableMethod =() => utilsgui = true, isTogglable = true, toolTip = "Disables the Utils GUI.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Networked Menu", method =() => disableNetworkedMenu = true, disableMethod =() => disableNetworkedMenu = false, isTogglable = true, toolTip = "Disables the menus networking.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Change Dig Size", overlapText = "Change Dig Size <color=grey>[<color=cyan>Normal</color>]</color>", enableMethod =() => ChangeDigSize(), method =() => ChangeDigSize(), disableMethod =() => ChangeDigSize(false), isIncremental = true, isTogglable = false, toolTip = "Changes the dig size for the VIM dig mod.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Float Menu", enableMethod =() => FloatMenu = true, disableMethod =() => FloatMenu = false, toolTip = "Makes the menu float infront of you.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Arraylist", method =() => Visual.Arraylist(), disableMethod =() => Visual.DisableArraylist(), toolTip = "Gives you a arraylist showing every mod you have enabled.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Flip Arraylist", enableMethod =() => fliparraylist = true, disableMethod =() => fliparraylist = false, toolTip = "Flips the array list.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Clear Notifications on Disconnect", enableMethod =() => clearNotificationsOnDisconnect = true, disableMethod =() => clearNotificationsOnDisconnect = false, toolTip = "Clears notifications when you leave a room." },
                new ButtonInfo { buttonText = "Flip Notifications", enableMethod =() => flipnotifications = true, disableMethod =() => flipnotifications = false, toolTip = "Flips the notifications.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Menu Animation", enableMethod =() => menuanimation = true, disableMethod =() => menuanimation = false, toolTip = "Makes the menu have a open and close animation.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "High Quality Text", enableMethod =() => highqualitytext = true, disableMethod =() => highqualitytext = false, toolTip = "Makes the menu text higher quality.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Lowercase Text", enableMethod =() => lowercasetext = true, disableMethod =() => lowercasetext = false, toolTip = "Makes the menu text lowercase.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Uppercase Text", enableMethod =() => uppercasetext = true, disableMethod =() => uppercasetext = false, toolTip = "Makes the menu text uppercase.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Menu Trail", enableMethod =() => menutrail = true, disableMethod =() => menutrail = false, toolTip = "Gives the menu a trail.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Pointer Trail", enableMethod =() => pointertrail = true, disableMethod =() => pointertrail = false, toolTip = "Gives the pointer a trail.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Pointer", enableMethod =() => disablepointer = true, disableMethod =() => disablepointer = false, toolTip = "Disables the menu pointer.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Menu Drop", enableMethod =() => disablemenudrop = true, disableMethod =() => disablemenudrop = false, toolTip = "Disables the menu dropping.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Auto Save", enableMethod =() => disableautosave = true, disableMethod =() => disableautosave = false, toolTip = "Disables the auto saving of menu settings that happen every 2 minutes.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Return Button", enableMethod =() => disableReturnButton = true, disableMethod =() => disableReturnButton = false, toolTip = "Disables the return button.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Search Button", enableMethod =() => disableSearchButton = true, disableMethod =() => disableSearchButton = false, toolTip = "Disables the search button.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Favorites Bind", enableMethod =() => disableFavBinds = true, disableMethod =() => disableFavBinds = false, toolTip = "Disables the favorite keybind so you cant favorite mods.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Quick Actions Bind", enableMethod =() => disableQuickactionsBinds = true, disableMethod =() => disableQuickactionsBinds = false, toolTip = "Disables the quick actions keybind so you cant favorite mods.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Ghost View", enableMethod =() => ghostView = false, disableMethod =() => ghostView = true, toolTip = "Disables the ghost view that spawns spheres on your hands while your rig is disabled.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Stump Text", enableMethod =() => stumptext = false, disableMethod =() => stumptext = true, toolTip = "Disables the stump text that is in stump.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Low Gravity Menu", enableMethod =() => lowgravitymenu = true, disableMethod =() => lowgravitymenu = false, toolTip = "Gives the menu low gravity.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Custom Menu Title", enableMethod =() => custommenutitle = true, disableMethod =() => custommenutitle = false, toolTip = "Makes the menu use a custom menu title you can edit in files.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Menu Title", enableMethod =() => disablemenutitle = true, disableMethod =() => disablemenutitle = false, toolTip = "Removes the menu title.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Page Number", enableMethod =() => disablepagenumber = true, disableMethod =() => disablepagenumber = false, toolTip = "Removes the page number.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Custom Boards", enableMethod =() => disablecustomboards = true, disableMethod =() => disablecustomboards = false, toolTip = "Disables the custom boards.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Room Notifications", enableMethod =() => disableroomnotifications = true, disableMethod =() => disableroomnotifications = false, toolTip = "Disables the room notifications.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Panic Prompt", enableMethod =() => panicPrompt = false, disableMethod =() => panicPrompt = true, toolTip = "Disables the prompt you get when you use panic.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "On GUI Menu", enableMethod =() => PcGui.OnGUIMenu = true, disableMethod =() => PcGui.OnGUIMenu = false, toolTip = "Makes the menu be a pc gui.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Show On Screen Stuff", enableMethod =() => PcGui.ShowOnScreenStuff = true, disableMethod =() => PcGui.ShowOnScreenStuff = false, enabled = true, toolTip = "Shows the stuff on your pc screen like arraylist/info bar.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Discord RPC", enableMethod =() => disablediscordrpc = true, disableMethod =() => disablediscordrpc = false, enabled = false, toolTip = "Disables the discord rpc.", ShowInArraylist = true },
                new ButtonInfo { buttonText = "Log Photon Events", enableMethod =() => logphotonevents = true, disableMethod =() => logphotonevents = false, toolTip = "Logs all photon events to your console." },
                new ButtonInfo { buttonText = "Save Preferences", method =() => SaveSettings(), isTogglable = false, toolTip = "Saves your settings and enabled mods to a file." },
                new ButtonInfo { buttonText = "Load Preferences", method =() => LoadSettings(), isTogglable = false, toolTip = "Loads your settings and enabled mods from a file." },
            },

            new ButtonInfo[] { // Movement Settings
                new ButtonInfo { buttonText = "Exit Movement Settings", method =() => CurrentCategoryName = "Settings", isTogglable = false, toolTip = "Returns to the main settings page for the menu." },

                new ButtonInfo { buttonText = "Change Fly Speed", overlapText = "Change Fly Speed <color=grey>[<color=cyan>Default</color>]</color>", enableMethod =() => ChangeFlySpeed(), method =() => ChangeFlySpeed(), disableMethod =() => ChangeFlySpeed(false), isIncremental = true,  isTogglable = false, toolTip = "Changes the fly speed.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Change Wall Walk Power", overlapText = "Change Wall Walk Power <color=grey>[<color=cyan>8.8f</color>]</color>", enableMethod =() => ChangeWallWalkAmount(), method =() => ChangeWallWalkAmount(), disableMethod =() => ChangeWallWalkAmount(false), isIncremental = true,  isTogglable = false, toolTip = "Changes the wall walk power.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Sticky Platforms", method =() => stickyplats = true, disableMethod =() => stickyplats = false, isTogglable = true, toolTip = "Makes the platforms sticky.", ShowInArraylist = false },
            },

            new ButtonInfo[] { // Visual Settings
                new ButtonInfo { buttonText = "Exit Visual Settings", method =() => CurrentCategoryName = "Settings", isTogglable = false, toolTip = "Returns to the main settings page for the menu." },

                new ButtonInfo { buttonText = "NameTags Follow Head", enableMethod =() => followheadmesh = true, disableMethod =() => followheadmesh = false, isTogglable = true, toolTip = "Makes the nametags follow the players head instead of there body.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Follow Menu Theme", enableMethod =() => followmenutheme = true, disableMethod =() => followmenutheme = false, isTogglable = true, toolTip = "Makes every visual mod follow the menu theme.", ShowInArraylist = false },
            },

            new ButtonInfo[] { // Advantage Settings
                new ButtonInfo { buttonText = "Exit Advantage Settings", method =() => CurrentCategoryName = "Settings", isTogglable = false, toolTip = "Returns to the main settings page for the menu." },

                new ButtonInfo { buttonText = "Change Tag Aura Range", overlapText = "Change Tag Aura Range <color=grey>[<color=cyan>Default</color>]</color>", enableMethod =() => ChangeTagAuraDistance(), method =() => ChangeTagAuraDistance(), disableMethod =() => ChangeTagAuraDistance(false), isIncremental = true,  isTogglable = false, toolTip = "Changes the tag aura range.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Instant Tag", enableMethod =() => instanttag = true, disableMethod =() => instanttag = false, toolTip = "Makes it so tag mods are instant.", ShowInArraylist = false },
            },

            new ButtonInfo[] { // Projectile Settings
                new ButtonInfo { buttonText = "Exit Projectile Settings", method =() => CurrentCategoryName = "Settings", isTogglable = false, toolTip = "Returns to the main settings page for the menu." },

                new ButtonInfo { buttonText = "Change Projectile Color", overlapText = "Change Projectile Color <color=grey>[<color=cyan>White</color>]</color>", enableMethod =() => ChangeProjectilesColor(), method =() => ChangeProjectilesColor(), disableMethod =() => ChangeProjectilesColor(false), isIncremental = true, isTogglable = false, toolTip = "Changes the color of the projectiles." },
                new ButtonInfo { buttonText = "Allow Colored Big Snowballs", enableMethod =() => allowbigsnowballcolor = true, disableMethod =() => allowbigsnowballcolor = false, isTogglable = true, toolTip = "Allows the growing snowballs to have a color." },
                new ButtonInfo { buttonText = "Change Projectile", overlapText = "Change Projectile <color=grey>[<color=cyan>Snowball</color>]</color>", isIncremental = true, enableMethod =() => Projectiles.ChangeProjectile(), method =() => Projectiles.ChangeProjectile(), disableMethod =() => Projectiles.ChangeProjectile(false), isTogglable = false, toolTip = "Changes the current projectile you use." },
                new ButtonInfo { buttonText = "Change Blind Color", overlapText = "Change Blind Color <color=grey>[<color=cyan>Black</color>]</color>", isIncremental = true, enableMethod =() => Projectiles.ChangeBlindColor(), method =() => Projectiles.ChangeBlindColor(), disableMethod =() => Projectiles.ChangeBlindColor(false), isTogglable = false, toolTip = "Changes the current color you use to blind people." },
            },
            new ButtonInfo[] { // Gunlib Settings
                new ButtonInfo { buttonText = "Exit Gunlib Settings", method =() => CurrentCategoryName = "Settings", isTogglable = false, toolTip = "Returns to the main settings page for the menu." },

                new ButtonInfo { buttonText = "Disable Gun Pointer", method =() => gunpointer = false, disableMethod =() => gunpointer = true, isTogglable = true, toolTip = "Disables the gun pointer.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Gun Line", method =() => gunline = false, disableMethod =() => gunline = true, isTogglable = true, toolTip = "Disables the gun line.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Change Gun Line", overlapText = "Change Gun Line <color=grey>[<color=cyan>Default</color>]</color>", enableMethod =() => ChangeGunline(), method =() => ChangeGunline(), disableMethod =() => ChangeGunline(false), isIncremental = true, isTogglable = false, toolTip = "Changes the gunline.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Swap Gun Hand", enableMethod =() => swapgunhand = true, disableMethod =() => swapgunhand = false, isTogglable = true, toolTip = "Swap what hand the gun is on.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Gun Target ESP", enableMethod =() => gunTargetESP = true, disableMethod =() => gunTargetESP = false, isTogglable = true, toolTip = "Makes who you shoot have a esp on them.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Gun Pointer Trail", enableMethod =() => gunPointerTrail = true, disableMethod =() => gunPointerTrail = false, isTogglable = true, toolTip = "Gives the gun pointer a trail.", ShowInArraylist = false },
            },

            new ButtonInfo[] { // Safety Settings
                new ButtonInfo { buttonText = "Exit Safety Settings", method =() => CurrentCategoryName = "Settings", isTogglable = false, toolTip = "Returns you back to the settings menu." },

                new ButtonInfo { buttonText = "Change Anti Report Range", overlapText = "Change Anti Report Range <color=grey>[<color=cyan>Massive</color>]</color>", enableMethod =() => ChangeAntiReportRange(), method =() => ChangeAntiReportRange(), disableMethod =() => ChangeAntiReportRange(false), isIncremental = true, isTogglable = false, toolTip = "Changes the range of the anti reports.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Disable Anti Report Visualizer", enableMethod =() => disableAntiReportVisualizer = true, disableMethod =() => disableAntiReportVisualizer = false, isTogglable = true, toolTip = "Changes the range of the anti reports.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Change Spoofed Fps", overlapText = "Change Spoofed Fps <color=grey>[<color=cyan>1</color>]</color>", enableMethod =() => ChangeFPS(), method =() => ChangeFPS(), disableMethod =() => ChangeFPS(false), isIncremental = true, isTogglable = false, toolTip = "Changes the spoofed fps.", ShowInArraylist = false },
                new ButtonInfo { buttonText = "Change Spoofed Ping", overlapText = "Change Spoofed Ping <color=grey>[<color=cyan>1</color>]</color>", enableMethod =() => ChangePing(), method =() => ChangePing(), disableMethod =() => ChangePing(false), isIncremental = true, isTogglable = false, toolTip = "Changes the spoofed ping.", ShowInArraylist = false },

            },

            new ButtonInfo[] { // Plugin Settings
                new ButtonInfo { buttonText = "Exit Plugin Settings", method =() => CurrentCategoryName = "Settings", isTogglable = false, toolTip = "Returns you back to the settings menu." },

                new ButtonInfo { buttonText = "Reload Plugins", method = PluginManager.ReloadPlugins, isTogglable = false, toolTip = "Reloads all of your plugins." }
            },

            new ButtonInfo[] { // Enabled
                new ButtonInfo { buttonText = "Exit Enabled", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },
            },

            new ButtonInfo[] { // Favorites
                new ButtonInfo { buttonText = "Exit Favorites", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },
            },

            new ButtonInfo[] { // Rooms
                new ButtonInfo { buttonText = "Exit Rooms", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },
            },

            new ButtonInfo[] { // Global
                new ButtonInfo { buttonText = "Exit Global", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },

                new ButtonInfo { buttonText = "Disable Map Triggers", enableMethod =() => GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab").SetActive(false), disableMethod =() => GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab").SetActive(true), toolTip = "Disables the map triggers so you can goto maps without loading them." },
                new ButtonInfo { buttonText = "Disable Network Triggers", enableMethod =() => NetworkTriggerPatch.enabled = true, disableMethod =() => NetworkTriggerPatch.enabled = false, toolTip = "Disables the network triggers so you can change maps without leaving/joining a room." },

                new ButtonInfo { buttonText = "Show Map Triggers", method =() => Important.ShowMapTriggers(true), disableMethod =() => Important.ShowMapTriggers(false), toolTip = "Shows you where the map triggers are." },
                new ButtonInfo { buttonText = "Show Network Triggers", method =() => Important.ShowNetworkTriggers(true), disableMethod =() => Important.ShowNetworkTriggers(false), toolTip = "Shows you where the network triggers are." },

                new ButtonInfo { buttonText = "Teleport To \"Stump\"", method =() => Movement.TeleportToMap("stump"), toolTip = "Teleports you to \"stump\".", isTogglable = false },
                new ButtonInfo { buttonText = "Teleport To \"City\"", method =() => Movement.TeleportToMap("city"), toolTip = "Teleports you to \"city\".", isTogglable = false },
                new ButtonInfo { buttonText = "Teleport To \"Canyons\"", method =() => Movement.TeleportToMap("canyons"), toolTip = "Teleports you to \"canyons\".", isTogglable = false },
                new ButtonInfo { buttonText = "Teleport To \"Caves\"", method =() => Movement.TeleportToMap("caves"), toolTip = "Teleports you to \"caves\".", isTogglable = false },
                new ButtonInfo { buttonText = "Teleport To \"Beach\"", method =() => Movement.TeleportToMap("beach"), toolTip = "Teleports you to \"beach\".", isTogglable = false },
                new ButtonInfo { buttonText = "Teleport To \"Clouds\"", method =() => Movement.TeleportToMap("clouds"), toolTip = "Teleports you to \"clouds\".", isTogglable = false },
                new ButtonInfo { buttonText = "Teleport To \"Mountains\"", method =() => Movement.TeleportToMap("mountains"), toolTip = "Teleports you to \"mountains\".", isTogglable = false },
                new ButtonInfo { buttonText = "Teleport To \"Mall\"", method =() => Movement.TeleportToMap("mall"), toolTip = "Teleports you to \"mall\".", isTogglable = false },

                new ButtonInfo { buttonText = "Connect to <color=grey>[<color=cyan>EU</color>]</color>", method =() => Important.ConnectToRegion("EU"), isTogglable = false, toolTip = "Connects to EU region." },
                new ButtonInfo { buttonText = "Connect to <color=grey>[<color=cyan>USW</color>]</color>", method =() => Important.ConnectToRegion("USW"), isTogglable = false, toolTip = "Connects to USW region." },
                new ButtonInfo { buttonText = "Connect to <color=grey>[<color=cyan>US</color>]</color>", method =() => Important.ConnectToRegion("US"), isTogglable = false, toolTip = "Connects to US region." },

                new ButtonInfo { buttonText = "Change Queue <color=grey>[<color=cyan>Default</color>]</color>", method =() => Important.ChangeQueue("DEFAULT"), isTogglable = false, toolTip = "Changes the current queue." },
                new ButtonInfo { buttonText = "Change Queue <color=grey>[<color=cyan>Minigames</color>]</color>", method =() => Important.ChangeQueue("MINIGAMES"), isTogglable = false, toolTip = "Changes the current queue." },
                new ButtonInfo { buttonText = "Change Queue <color=grey>[<color=cyan>Competitive</color>]</color>", method =() => Important.ChangeQueue("COMPETITIVE"), isTogglable = false, toolTip = "Changes the current queue." },
                new ButtonInfo { buttonText = "Change Queue <color=grey>[<color=cyan>ARRAKIS</color>]</color>", method =() => Important.ChangeQueue("ARRAKIS"), isTogglable = false, toolTip = "Changes the current queue." },

                new ButtonInfo { buttonText = "Change Gamemode <color=grey>[<color=cyan>Casual</color>]</color>", method =() => Important.ChangeGamemode("CASUAL"), isTogglable = false, toolTip = "Changes your current gamemode." },
                new ButtonInfo { buttonText = "Change Gamemode <color=grey>[<color=cyan>Infection</color>]</color>", method =() => Important.ChangeGamemode("INFECTION"), isTogglable = false, toolTip = "Changes your current gamemode." },
                new ButtonInfo { buttonText = "Change Gamemode <color=grey>[<color=cyan>Hunt</color>]</color>", method =() => Important.ChangeGamemode("HUNTDOWW"), isTogglable = false, toolTip = "Changes your current gamemode." },
                new ButtonInfo { buttonText = "Change Gamemode <color=grey>[<color=cyan>Paintbrawl</color>]</color>", method =() => Important.ChangeGamemode("PAINTBRAWL"), isTogglable = false, toolTip = "Changes your current gamemode." },
                new ButtonInfo { buttonText = "Change Gamemode <color=grey>[<color=cyan>Guardian</color>]</color>", method =() => Important.ChangeGamemode("GUARDIAN"), isTogglable = false, toolTip = "Changes your current gamemode." },
                new ButtonInfo { buttonText = "Change Gamemode <color=grey>[<color=cyan>GhostTag</color>]</color>", method =() => Important.ChangeGamemode("GHOST"), isTogglable = false, toolTip = "Changes your current gamemode." },
                new ButtonInfo { buttonText = "Change Gamemode <color=grey>[<color=cyan>Ambush</color>]</color>", method =() => Important.ChangeGamemode("AMBUSH"), isTogglable = false, toolTip = "Changes your current gamemode." },
                new ButtonInfo { buttonText = "Change Gamemode <color=grey>[<color=cyan>FreezeTag</color>]</color>", method =() => Important.ChangeGamemode("FREEZETAG"), isTogglable = false, toolTip = "Changes your current gamemode." },
                new ButtonInfo { buttonText = "Change Gamemode <color=grey>[<color=cyan>Prop Hunt</color>]</color>", method =() => Important.ChangeGamemode("PROPHUNT"), isTogglable = false, toolTip = "Changes your current gamemode." },
                new ButtonInfo { buttonText = "Change Gamemode <color=grey>[<color=cyan>Super Infection</color>]</color>", method =() => Important.ChangeGamemode("SUPERINFECT"), isTogglable = false, toolTip = "Changes your current gamemode." },
                new ButtonInfo { buttonText = "Change Gamemode <color=grey>[<color=cyan>Custom</color>]</color>", method =() => Important.ChangeGamemode("CUSTOM"), isTogglable = false, toolTip = "Changes your current gamemode." },
            },

            new ButtonInfo[] { // Important
                new ButtonInfo { buttonText = "Exit Important", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },

                new ButtonInfo { buttonText = "Quit GTAG", method =() => Important.QuitGame(), isTogglable = false, toolTip = "Quits your game." },
                new ButtonInfo { buttonText = "First Person", method =() => Important.FirstPerson(), disableMethod =() => Important.DisableFirstPerson(), isTogglable = true, toolTip = "Makes your pc view see what your vr sees." },
                new ButtonInfo { buttonText = "Unlock Comp", method =() => Important.UnlockComp(), isTogglable = false, toolTip = "Unlocks the comp queue." },
                new ButtonInfo { buttonText = "Clear Notifications", method =() => Important.CLearNotis(), isTogglable = false, toolTip = "Clears your notifications." },
                new ButtonInfo { buttonText = "Accept TOS", method =() => TOSPatches.enabled = true, disableMethod =() => TOSPatches.enabled = false, isTogglable = true, toolTip = "Acccepts the tos." },
                new ButtonInfo { buttonText = "Disable AFK Kick", method =() => Important.DisableAFKKick(), disableMethod =() => Important.EnsableAFKKick(), isTogglable = true, toolTip = "Disables the afk kick." },
                new ButtonInfo { buttonText = "Disable Quit Box", method =() => AntiQuitBox.disable = true, disableMethod =() => AntiQuitBox.disable = false, isTogglable = true, toolTip = "Disables the quit box." },
                new ButtonInfo { buttonText = "PC Button Click", method =() => Important.ButtonClick(), isTogglable = true, toolTip = "Lets you click buttons with your mouse." },
                new ButtonInfo { buttonText = "Reauth", method =() => Important.Reauth(), isTogglable = false, toolTip = "Reauths you to gtag." },
                new ButtonInfo { buttonText = "Disable Wind Barriers", enableMethod =() => WindPatch.enabled = true, method =() => Important.DisableWindBarriers(), disableMethod =() => { WindPatch.enabled = false; Important.EnableWindBarriers(); }, isTogglable = true, toolTip = "Disables the wind barriers." },
                new ButtonInfo { buttonText = "Unload Menu", method =() => Important.UnloadMenu(), isTogglable = false, toolTip = "Unloads the menu and you wont be able to use it until you restart." },
            },

            new ButtonInfo[] { // Computer
                new ButtonInfo { buttonText = "Exit Computer", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },

                new ButtonInfo { buttonText = "Disconnect", method =() => Important.Disconnect(), isTogglable = false, toolTip = "Disconnects you from the current lobby." },
                new ButtonInfo { buttonText = "Join Random", method =() => Important.JoinRandomRoom(), isTogglable = false, toolTip = "Attempts to join a random room." },
                new ButtonInfo { buttonText = "Reconnect", method =() => Important.Reconnect(), isTogglable = false, toolTip = "Reconnects you to the room you were in." },
                new ButtonInfo { buttonText = "Join 'ARRAKIS'", method =() => Important.JoinRoom("ARRAKIS"), isTogglable = false, toolTip = "Attempts to make you join a certain room." },
                new ButtonInfo { buttonText = "Join '1'", method =() => Important.JoinRoom("1"), isTogglable = false, toolTip = "Attempts to make you join a certain room." },
                new ButtonInfo { buttonText = "Join 'MODS'", method =() => Important.JoinRoom("MODS"), isTogglable = false, toolTip = "Attempts to make you join a certain room." },
                new ButtonInfo { buttonText = "Join 'MOD'", method =() => Important.JoinRoom("MOD"), isTogglable = false, toolTip = "Attempts to make you join a certain room." },
                new ButtonInfo { buttonText = "Join 'SREN17'", method =() => Important.JoinRoom("SREN17"), isTogglable = false, toolTip = "Attempts to make you join a certain room." },
                new ButtonInfo { buttonText = "Join 'SREN18'", method =() => Important.JoinRoom("SREN18"), isTogglable = false, toolTip = "Attempts to make you join a certain room." },
                new ButtonInfo { buttonText = "Join 'PBBV'", method =() => Important.JoinRoom("PBBV"), isTogglable = false, toolTip = "Attempts to make you join a certain room." },
                new ButtonInfo { buttonText = "Join 'GROUND'", method =() => Important.JoinRoom("GROUND"), isTogglable = false, toolTip = "Attempts to make you join a certain room." },
                new ButtonInfo { buttonText = "Join 'LUCIO'", method =() => Important.JoinRoom("LUCIO"), isTogglable = false, toolTip = "Attempts to make you join a certain room." },
                new ButtonInfo { buttonText = "Join 'SOLAR'", method =() => Important.JoinRoom("SOLAR"), isTogglable = false, toolTip = "Attempts to make you join a certain room." },
                new ButtonInfo { buttonText = "Join Random Private Room", method =() => Important.JoinRoom(Main.GenerateRandomString(UnityEngine.Random.Range(4, 8))), isTogglable = false, toolTip = "Attempts to make you join a certain room." },
                new ButtonInfo { buttonText = "Create '<color=red>ARRAKIS</color>'", method =() => Important.CreatePublicLobby("<color=red>ARRAKIS</color>"), isTogglable = false, toolTip = "Attempts to make a public room." },
                new ButtonInfo { buttonText = "Create '❤️❤️❤️'", method =() => Important.CreatePublicLobby("<size=90><color=red>❤️❤️❤️</color></size>"), isTogglable = false, toolTip = "Attempts to make a public room." },
                new ButtonInfo { buttonText = $"Create '{PluginInfo.DiscordLink}'", method =() => Important.CreatePublicLobby($"<color=red>{PluginInfo.DiscordLink}</color>"), isTogglable = false, toolTip = "Attempts to make a public room." },
            },

            new ButtonInfo[] { // Safety
                new ButtonInfo { buttonText = "Exit Safety", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },

                new ButtonInfo { buttonText = "Anti Report <color=grey>[<color=cyan>Disconnect</color>]</color>", method =() => Safety.AntiReportDisconnect(), disableMethod =() => Safety.DestroyARSphere(), isTogglable = true, toolTip = "Disconnects you from the lobby when someone tries to report you." },
                new ButtonInfo { buttonText = "Anti Report <color=grey>[<color=cyan>Reconnect</color>]</color>", method =() => Safety.AntiReportReconnect(), disableMethod =() => Safety.DestroyARSphere(), isTogglable = true, toolTip = "Reconnects you to the current lobby when someone tries to report you." },
                new ButtonInfo { buttonText = "Anti Report <color=grey>[<color=cyan>Join Random</color>]</color>", method =() => Safety.AntiReportJoinRandom(), disableMethod =() => Safety.DestroyARSphere(), isTogglable = true, toolTip = "Attempts to make you join a random room when someone tries to report you." },
                new ButtonInfo { buttonText = "Flush RPC's", method =() => Safety.RPCProc(), isTogglable = false, toolTip = "Flushes all your rpc calls." },
                new ButtonInfo { buttonText = "No Finger Movement", method =() => Safety.NoFingerMovement(), isTogglable = true, toolTip = "Disables your finger movement." },
                new ButtonInfo { buttonText = "Panic", method =() => Safety.Panic(), isTogglable = false, toolTip = "Disables every mod on the menu." },
                new ButtonInfo { buttonText = "Bypass VC Ban", method =() => Safety.BypassVCBan(), isTogglable = false, toolTip = "Attemts to bypass the voice bans." },
                new ButtonInfo { buttonText = "Anti Staff", method =() => Safety.AntiModerator(), isTogglable = true, toolTip = "Disconnects you when someone with staff items joins." },
                new ButtonInfo { buttonText = "Anti Content Creator", method =() => Safety.AntiContentCreator(), isTogglable = true, toolTip = "Disconnects you when someone with a content creator badge joins." },
                new ButtonInfo { buttonText = "Anti Crash", method =() => Safety.AntiCrash(), isTogglable = true, toolTip = "Attemts to bypass laggers/crashers by clearing the rpc queue." },
                new ButtonInfo { buttonText = "Anti Guardian Grab", enableMethod =() => GardGrabPatch.enabled = true, disableMethod =() => GardGrabPatch.enabled = false, isTogglable = true, toolTip = "Dosnt let players pick you up in guardian." },
                new ButtonInfo { buttonText = "Anti Knockback", enableMethod =() => KnockbackPatch.enabled = true, disableMethod =() => KnockbackPatch.enabled = false, isTogglable = true, toolTip = "Dosnt let anything knock you back." },
                new ButtonInfo { buttonText = "Spoof Platform", enableMethod =() => Safety.SpoofPlatform(true), disableMethod =() => Safety.SpoofPlatform(false), isTogglable = true, toolTip = "Spoofs your platform to mod checkers." },
                new ButtonInfo { buttonText = "Spoof Support Page", method =() => Safety.SpoofSupportPage(), isTogglable = true, toolTip = "Spoofs the support page on the computer." },
                new ButtonInfo { buttonText = "Anti Memory Leak", method =() => Safety.AntiMemoryLeak(), isTogglable = true, toolTip = "Makes it so you cant have memory leaks." },
                new ButtonInfo { buttonText = "Show AntiCheat Reports <color=grey>[<color=cyan>OTHERS</color>]</color>", enableMethod =() => showanticheatreports = true, disableMethod =() => showanticheatreports = false, toolTip = "Logs everyone elses anti cheat reports in your console." },
                new ButtonInfo { buttonText = "Show AntiCheat Reports <color=grey>[<color=cyan>SELF</color>]</color>", enableMethod =() => showanticheatreportself = true, disableMethod =() => showanticheatreportself = false, toolTip = "Logs your own anti cheat reports in your console." },
                new ButtonInfo { buttonText = "Anti Stump Kick", enableMethod =() => GroupPatch.enabled = true, disableMethod =() => GroupPatch.enabled = false, toolTip = "Dosnt let you get kicked from group kick." },
                new ButtonInfo { buttonText = "Board Spoof", method =() => Safety.BoardSpoof(), toolTip = "Spoofs your name and color every 30 secounds." },
                new ButtonInfo { buttonText = "Nuke Mod Checkers", enableMethod =() => Safety.NukeModCheckers(), toolTip = "Tells mod checkers that you have alot of mods." },
                new ButtonInfo { buttonText = "Anti Overlay", method =() => Safety.AntiOverlay(), toolTip = "Makes it so you dont go invis when you open your steam dashboard." },
                new ButtonInfo { buttonText = "Spoof Fps", enableMethod =() => Arrakis.Patches.Patchers.GTPlayerStatsP.SpoofFPS = true, disableMethod =() => Arrakis.Patches.Patchers.GTPlayerStatsP.SpoofFPS = false, isTogglable = true, toolTip = "Spoofs your fps to mod checkers."},
                new ButtonInfo { buttonText = "Spoof Ping", enableMethod =() => Arrakis.Patches.Patchers.GTPlayerStatsP.SpoofPing = true, disableMethod =() => Arrakis.Patches.Patchers.GTPlayerStatsP.SpoofPing = false, isTogglable = true, toolTip = "Spoofs your ping to mod checkers."},

            },

            new ButtonInfo[] { // Movement
                new ButtonInfo { buttonText = "Exit Movement", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },

                new ButtonInfo { buttonText = "Fly <color=grey>[<color=cyan>A</color>]</color>", method =() => Movement.Fly(), isTogglable = true, toolTip = "Lets you fly while holding <color=grey>[<color=cyan>A</color>]</color>." },
                new ButtonInfo { buttonText = "Hand Fly <color=grey>[<color=cyan>A</color>]</color>", method =() => Movement.HandFly(), isTogglable = true, toolTip = "Lets you fly while holding <color=grey>[<color=cyan>A</color>]</color>." },
                new ButtonInfo { buttonText = "Trigger Fly <color=grey>[<color=cyan>T</color>]</color>", method =() => Movement.TriggerFly(), isTogglable = true, toolTip = "Lets you fly while holding <color=grey>[<color=cyan>T</color>]</color>." },
                new ButtonInfo { buttonText = "Noclip Fly <color=grey>[<color=cyan>A</color>]</color>", method =() => Movement.NoclipFly(), isTogglable = true, toolTip = "Lets you fly while with noclip holding <color=grey>[<color=cyan>A</color>]</color>." },
                new ButtonInfo { buttonText = "Iron Monkey", method =() => Movement.ExcelFly(), isTogglable = true, toolTip = "Lets you fly like iron man." },
                new ButtonInfo { buttonText = "Platforms", method =() => Movement.Platforms(), isTogglable = true, toolTip = "Lets you walk on air with platforms." },
                new ButtonInfo { buttonText = "Trigger Platforms", method =() => Movement.Platforms(true), isTogglable = true, toolTip = "Lets you walk on air with platforms." },
                new ButtonInfo { buttonText = "Invis Platforms", method =() => Movement.Platforms(false, true), isTogglable = true, toolTip = "Lets you walk on air with platforms." },
                new ButtonInfo { buttonText = "Platform Spam <color=grey>[<color=cyan>G</color>]</color>", method =() => Movement.PlatformSpam(), isTogglable = true, toolTip = "Lets you spam platforms." },
                new ButtonInfo { buttonText = "Frozone <color=grey>[<color=cyan>G</color>]</color>", method =() => Movement.Frozone(), isTogglable = true, toolTip = "Lets you become frozone." },
                new ButtonInfo { buttonText = "WASD Fly", method =() => Movement.WasdFly(), isTogglable = true, toolTip = "Lets you move around with wasd." },
                new ButtonInfo { buttonText = "No Clip <color=grey>[<color=cyan>T</color>]</color>", method =() => Movement.NoClip(), isTogglable = true, toolTip = "Lets you go through objects." },
                new ButtonInfo { buttonText = "No Tag Freeze", method =() => Movement.NoTagFreeze(true), isTogglable = true, toolTip = "Disables the tag freeze you get when you get tagged." },
                new ButtonInfo { buttonText = "Force Tag Freeze", method =() => Movement.NoTagFreeze(false), disableMethod =() => Movement.NoTagFreeze(true), isTogglable = true, toolTip = "Enables the tag freeze you get when you get tagged." },
                new ButtonInfo { buttonText = "Steam Long Arms", method =() => Movement.SteamLongArms(), disableMethod =() => Movement.DisableLongArms(), isTogglable = true, toolTip = "Makes your arms longer." },
                new ButtonInfo { buttonText = "Long Arms", method =() => Movement.LongArms(), disableMethod =() => Movement.DisableLongArms(), isTogglable = true, toolTip = "Makes your arms longer." },
                new ButtonInfo { buttonText = "TP Gun", method =() => Movement.TPGun(), isTogglable = true, toolTip = "Lets you teleport with a gun." },
                new ButtonInfo { buttonText = "PSA <color=grey>[<color=cyan>A</color>]</color>", method =() => Movement.PSA(), isTogglable = true, toolTip = "Lets you psa." },
                new ButtonInfo { buttonText = "Mosa Boost", method =() => Movement.MosaBoost(), isTogglable = true, toolTip = "Gives you a slight speed boost." },
                new ButtonInfo { buttonText = "Speed Boost", method =() => Movement.SpeedBoost(), isTogglable = true, toolTip = "Gives you a speed boost." },
                new ButtonInfo { buttonText = "Extreme Speed Boost", method =() => Movement.ExtremeSpeedBoost(), isTogglable = true, toolTip = "Gives you a extreme speed boost." },
                new ButtonInfo { buttonText = "Slide Control", method =() => Movement.SlideControl(), disableMethod =() => Movement.FixSlideControl(), isTogglable = true, toolTip = "Gives you more control on ice." },
                new ButtonInfo { buttonText = "Wall Walk <color=grey>[<color=cyan>G</color>]</color>", method =() => Movement.WallWalk(), isTogglable = true, toolTip = "Lets you walk on the walls easier." },
                new ButtonInfo { buttonText = "Kayflock <color=grey>[<color=cyan>T</color>]</color>", method =() => Movement.AutoKayflock(), isTogglable = true, toolTip = "Lets you automaticly kayflock." },
                new ButtonInfo { buttonText = "Checkpoint", method =() => Movement.CheckPoint(), disableMethod =() => Movement.DisableCheckPoint(), isTogglable = true, toolTip = "Lets you spawn a check point and teleport back to it at anytime." },
                new ButtonInfo { buttonText = "Piggyback Gun", method =() => Movement.PiggybackGun(), isTogglable = true, toolTip = "Lets you piggyback the person who you shoot." },
                new ButtonInfo { buttonText = "Follow Player Gun", method =() => Movement.FollowPlayerGun(), isTogglable = true, toolTip = "Lets you follow the person who you shoot." },
                new ButtonInfo { buttonText = "Rig Follow Player Gun", method =() => Movement.RigFollowPlayerGun(), disableMethod =() => Movement.FixRig(), isTogglable = true, toolTip = "Lets your vrrig follow the person who you shoot." },
                new ButtonInfo { buttonText = "Pull Mod <color=grey>[<color=cyan>RJ</color>]</color>", method =() => Movement.PullMod(), isTogglable = true, toolTip = "Give you fake pull, bearly pass checks." },
                new ButtonInfo { buttonText = "Pull Boost <color=grey>[<color=cyan>RJ</color>]</color>", method =() => Movement.PullBoost(), isTogglable = true, toolTip = "Give you fake pull, might pass checks." },
                new ButtonInfo { buttonText = "Predictions", enableMethod =() => Movement.CreatePredThingy(), method =() => Movement.Preds(), disableMethod =() => Movement.RemovePredThingy(), isTogglable = true, toolTip = "Give you predictions, fails checks." },
            },

            new ButtonInfo[] { // VRRig
                new ButtonInfo { buttonText = "Exit VRRig", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },

                new ButtonInfo { buttonText = "Ghost Monkey <color=grey>[<color=cyan>B</color>]</color>", method =() => Movement.GhostMonkey(), disableMethod =() => Movement.FixRig(), isTogglable = true, toolTip = "Makes you become a ghost when holding <color=grey>[<color=cyan>B</color>]</color>." },
                new ButtonInfo { buttonText = "Toggle Ghost Monkey <color=grey>[<color=cyan>B</color>]</color>", method =() => Movement.ToggleGhostMonkey(), disableMethod =() => Movement.FixRig(), isTogglable = true, toolTip = "Makes you become a ghost when you press <color=grey>[<color=cyan>B</color>]</color>." },
                new ButtonInfo { buttonText = "Invis Monkey <color=grey>[<color=cyan>A</color>]</color>", method =() => Movement.InvisMonke(), disableMethod =() => Movement.FixRig(), isTogglable = true, toolTip = "Makes you become invis when holding <color=grey>[<color=cyan>B</color>]</color>." },
                new ButtonInfo { buttonText = "Toggle Invis Monkey <color=grey>[<color=cyan>A</color>]</color>", method =() => Movement.ToggleInvisMonkey(), disableMethod =() => Movement.FixRig(), isTogglable = true, toolTip = "Makes you become invis when you press <color=grey>[<color=cyan>B</color>]</color>." },
                new ButtonInfo { buttonText = "Spaz Rig", method =() => Movement.SpazRig(), isTogglable = true, toolTip = "Makes your rig spaz out." },
                new ButtonInfo { buttonText = "Spaz Hands", method =() => Movement.SpazHands(), isTogglable = true, toolTip = "Makes your hands spaz out." },
                new ButtonInfo { buttonText = "Spaz Head", method =() => Movement.SpazHead(), isTogglable = true, toolTip = "Makes your head spaz out." },
                new ButtonInfo { buttonText = "Rig Gun", method =() => Movement.RigGun(), disableMethod =() => Movement.FixRig(), isTogglable = true, toolTip = "Moves your rig to where you shoot." },
                new ButtonInfo { buttonText = "Grab Rig <color=grey>[<color=cyan>G</color>]</color>", method =() => Movement.GrabRig(), disableMethod =() => Movement.FixRig(), isTogglable = true, toolTip = "Lets you grab your rig when holding <color=grey>[<color=cyan>G</color>]</color>." },
                new ButtonInfo { buttonText = "Smooth Rig", enableMethod =() => PhotonNetwork.SerializationRate = 35, disableMethod =() => PhotonNetwork.SerializationRate = 10, isTogglable = true, toolTip = "Makes your rig smooth to other players." },
                new ButtonInfo { buttonText = "Fake Full Body Tracking", enableMethod =() => TorsoPatch.VRRigLateUpdate += Movement.FakeFBT, disableMethod =() => TorsoPatch.VRRigLateUpdate -= Movement.FakeFBT, isTogglable = true, toolTip = "Makes it look like you have full body tracking." },
                new ButtonInfo { buttonText = "Spaz Full Body Tracking", enableMethod =() => TorsoPatch.VRRigLateUpdate += Movement.SpazBody, disableMethod =() => TorsoPatch.VRRigLateUpdate -= Movement.SpazBody, isTogglable = true, toolTip = "Spazzes the full body tracking." },
                new ButtonInfo { buttonText = "Flip", enableMethod =() => TorsoPatch.VRRigLateUpdate += Movement.Flip, disableMethod =() => { TorsoPatch.VRRigLateUpdate -= Movement.Flip; Movement.StopFlip(); }, isTogglable = true, toolTip = "Lets you flip when pressing your trigger." },
                new ButtonInfo { buttonText = "Smooth Body", method =() => Movement.SmoothBody(), disableMethod =() => Movement.ToggleTorsoPatch(false), isTogglable = true, toolTip = "Smooths your vrrigs body." },
            },

            new ButtonInfo[] { // Visual
                new ButtonInfo { buttonText = "Exit Visual", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },

                new ButtonInfo { buttonText = "Chams", method =() => Visual.Chams(0), disableMethod =() => Visual.DisableChams(), isTogglable = true, toolTip = "Lets you see everyone through walls." },
                new ButtonInfo { buttonText = "Chams V2", method =() => Visual.Chams(1), disableMethod =() => Visual.DisableChams(), isTogglable = true, toolTip = "Lets you see everyone through only objects and walls." },
                new ButtonInfo { buttonText = "Tracers", method =() => Visual.Tracers(), disableMethod =() => Visual.DisableTracers(), isTogglable = true, toolTip = "Points lines at everyone." },
                new ButtonInfo { buttonText = "Box ESP", method =() => Visual.BoxESP(), disableMethod =() => Visual.DisableBoxESP(), isTogglable = true, toolTip = "Gives people boxes on them." },
                new ButtonInfo { buttonText = "Hollow Box ESP", method =() => Visual.HollowBoxESP(), disableMethod =() => Visual.DisableHollowBoxESP(), isTogglable = true, toolTip = "Gives people hollow boxes on them."},
                new ButtonInfo { buttonText = "Bone ESP", method =() => Visual.BoneESP(), disableMethod =() => Visual.DisableBoneESP(), isTogglable = true, toolTip = "Gives people bones on there body." },
                new ButtonInfo { buttonText = "Bone V2 ESP", method =() => Visual.Chams(2), disableMethod =() => Visual.DisableChams(), isTogglable = true, toolTip = "Gives people bones on there body that you can only see behind walls." },
                new ButtonInfo { buttonText = "Self Bone ESP", method =() => Visual.SelfBoneESP(), disableMethod =() => Visual.DisableSelfBoneESP(), isTogglable = true, toolTip = "Gives you bones on your body." },
                new ButtonInfo { buttonText = "Wireframe ESP", method =() => Visual.WireframeESP(), disableMethod =() => Visual.DisableWireframeESP(), isTogglable = true, toolTip = "Wireframe ESP on others." },
                new ButtonInfo { buttonText = "Self Wireframe ESP", enableMethod =() => Visual.EnableSelfWireframeMesh(), method =() => Visual.UpdateSelfWireframeColor(), disableMethod =() => Visual.DisableSelfWireframeMesh(), isTogglable = true, toolTip = "Wireframe ESP on yourself." },
                new ButtonInfo { buttonText = "Name Tags", method =() => Visual.NameTags(), disableMethod =() => Visual.DisableNameTags(), isTogglable = true, toolTip = "Gives everyone a nametag that shows tthey'rehere name." },
                new ButtonInfo { buttonText = "ID Name Tags", method =() => Visual.IdNameTags(), disableMethod =() => Visual.DisableIdNameTags(), isTogglable = true, toolTip = "Gives everyone a nametag that shows they're userid." },
                new ButtonInfo { buttonText = "Platform Name Tags", method =() => Visual.PlatformNameTags(), disableMethod =() => Visual.DisablePlatformNameTags(), isTogglable = true, toolTip = "Gives everyone a nametag that shows they're platform." },
                new ButtonInfo { buttonText = "Fps Name Tags", method =() => Visual.FpsNameTags(), disableMethod =() => Visual.DisableFpsNameTags(), isTogglable = true, toolTip = "Gives everyone a nametag that shows they're fps." },
                new ButtonInfo { buttonText = "Tagged Name Tags", method =() => Visual.TaggedNameTags(), disableMethod =() => Visual.DisableTaggedNameTags(), isTogglable = true, toolTip = "Gives everyone a nametag that shows if they're tagged." },
                new ButtonInfo { buttonText = "Morning Time", method =() => BetterDayNightManager.instance.SetTimeOfDay(1), toolTip = "Sets your time of day to morning." },
                new ButtonInfo { buttonText = "Day Time", method =() => BetterDayNightManager.instance.SetTimeOfDay(3), toolTip = "Sets your time of day to daytime." },
                new ButtonInfo { buttonText = "Evening Time", method =() => BetterDayNightManager.instance.SetTimeOfDay(7), toolTip = "Sets your time of day to evening." },
                new ButtonInfo { buttonText = "Night Time", method =() => BetterDayNightManager.instance.SetTimeOfDay(0), toolTip = "Sets your time of day to night." },
                new ButtonInfo { buttonText = "Snowfall", enableMethod =() => Visual.Snowfall(true), disableMethod =() => Visual.Snowfall(false), toolTip = "Toggles the snow." },
                new ButtonInfo { buttonText = "Rain", enableMethod =() => Visual.Rain(true), disableMethod =() => Visual.Rain(false), toolTip = "Toggles the Rain." },
                new ButtonInfo { buttonText = "No Leaves", enableMethod =() => Visual.NoLeaves(), disableMethod =() => Visual.DisableNoLeaves(), toolTip = "Removes all the leaves in the game." },
                new ButtonInfo { buttonText = "Clear Weather", method =() => Visual.ClearWeather(), toolTip = "Clears the weather.", isTogglable = true },
                new ButtonInfo { buttonText = "Spectate Gun", method =() => Visual.SpectateGun(), toolTip = "Spectates who ever you shoot.", isTogglable = true },
                new ButtonInfo { buttonText = "Hand Trails", enableMethod =() => Visual.EnableHandTrails(), method =() => Visual.HandTrails(), disableMethod =() => Visual.DestroyHandTrails(), toolTip = "Puts trails on your hands.", isTogglable = true },
                new ButtonInfo { buttonText = "Player Info Hud", enableMethod =() => Visual.PlayerInfo(), disableMethod =() => Visual.CleanupPlayerInfo(), toolTip = "Displays info on your hud.", isTogglable = true },
                new ButtonInfo { buttonText = "Bug ESP", method =() => Visual.EntityESP(ThrowableBug.BugName.DougTheBug), disableMethod =() => Visual.DisableEntityESP(ThrowableBug.BugName.DougTheBug), toolTip = "Puts esp on the bug.", isTogglable = true },
                new ButtonInfo { buttonText = "Bat ESP", method =() => Visual.EntityESP(ThrowableBug.BugName.MattTheBat), disableMethod =() => Visual.DisableEntityESP(ThrowableBug.BugName.MattTheBat), toolTip = "Puts esp on the bat.", isTogglable = true },
                new ButtonInfo { buttonText = "Halloween Fog", enableMethod =() => Visual.EnableFog(), disableMethod =() => Visual.DisableFog(), toolTip = "Enables the halloween fog.", isTogglable = true },
                new ButtonInfo { buttonText = "Ring", method =() => Visual.Ring(), disableMethod =() => Visual.DisableRing(), toolTip = "Makes other people have a ring under them.", isTogglable = true },
                new ButtonInfo { buttonText = "Halo", method =() => Visual.Halo(), disableMethod =() => Visual.DisableHalo(), toolTip = "Makes other people have a halo above them like an angel.", isTogglable = true },
                new ButtonInfo { buttonText = "Fps boost", enableMethod =() => QualitySettings.globalTextureMipmapLimit = int.MaxValue, disableMethod =() => QualitySettings.globalTextureMipmapLimit = 1, toolTip = "Makes everything low quality to boost your FPS.", isTogglable = true },
                new ButtonInfo { buttonText = "Full Bright", enableMethod =() => Visual.fullBright(true), disableMethod =() => Visual.fullBright(false), toolTip = "Makes everything bright.", isTogglable = true },
                new ButtonInfo { buttonText = "Breadcrumbs", method =() => Visual.Breadcrumbs(), disableMethod =() => Visual.DisableBreadcrumbs(), toolTip = "Puts trails on players.", isTogglable = true },
            },

            new ButtonInfo[] { // Advantage
                new ButtonInfo { buttonText = "Exit Advantage", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },

                new ButtonInfo { buttonText = "Tag All", method =() => Advantage.TagAll(), disableMethod =() => Movement.FixRig(), isTogglable = true, toolTip = "Tags everyone in the current lobby." },
                new ButtonInfo { buttonText = "Tag Aura", method =() => Advantage.TagAura(), isTogglable = true, toolTip = "Tags the closest person." },
                new ButtonInfo { buttonText = "Tag Self", method =() => Advantage.TagSelf(), disableMethod =() => Movement.FixRig(), isTogglable = true, toolTip = "Tags your self if your not tagged." },
                new ButtonInfo { buttonText = "Tag Gun", method =() => Advantage.TagGun(), disableMethod =() => Movement.FixRig(), isTogglable = true, toolTip = "Tags the person you shoot." },
                new ButtonInfo { buttonText = "Flick Tag Gun", method =() => Advantage.FlickTagGun(), isTogglable = true, toolTip = "Lets you flick tag with a gun." },
                new ButtonInfo { buttonText = "Hitboxes", enableMethod =() => HitBoxesPatch.enabled = true, method =() => Advantage.Hitboxes(), disableMethod =() => { Advantage.DestroyHitboxes(); HitBoxesPatch.enabled = false; }, isTogglable = true, toolTip = "Gives you hitboxes that show where your hands are." },
                new ButtonInfo { buttonText = "No Tag On Join", method =() => Advantage.NoTagOnJoin(), disableMethod =() => Advantage.TagOnJoin(), isTogglable = true, toolTip = "Dosnt tag you when you join a room." },
                new ButtonInfo { buttonText = "Fake Lag", enableMethod =() => PhotonNetwork.SerializationRate = 1, disableMethod =() => PhotonNetwork.SerializationRate = 10, isTogglable = true, toolTip = "Makes your rig really laggy to other players." },
                new ButtonInfo { buttonText = "No Tag Limit", enableMethod =() => GorillaTagger.Instance.maxTagDistance = float.MaxValue, disableMethod =() => GorillaTagger.Instance.maxTagDistance = 2.2f, isTogglable = true, toolTip = "Makes it so flick tagging is back." },
                new ButtonInfo { buttonText = "Anti Tag", method =() => Advantage.AntiTag(), isTogglable = true, toolTip = "Makes it so you cant be tagged." },
                new ButtonInfo { buttonText = "Blink <color=grey>[<color=cyan>T</color>]</color>", method =() => Advantage.Blink(), disableMethod =() => Advantage.StopBlinking(), isTogglable = true, toolTip = "Lets you freeze other players when holding <color=grey>[<color=cyan>T</color>]</color>." },
            },

            new ButtonInfo[] { // Sound
                new ButtonInfo { buttonText = "Exit Sound", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },

                new ButtonInfo { buttonText = "Button Spam <color=grey>[<color=cyan>T</color>]</color>", method =() => Sound.SoundSpam(67), isTogglable = true, toolTip = "Spams the button sound when you hold <color=grey>[<color=cyan>T</color>]</color>." },
                new ButtonInfo { buttonText = "Keyboard Spam <color=grey>[<color=cyan>T</color>]</color>", method =() => Sound.SoundSpam(66), isTogglable = true, toolTip = "Spams the keyboard sound when you hold <color=grey>[<color=cyan>T</color>]</color>." },
                new ButtonInfo { buttonText = "Snow Spam <color=grey>[<color=cyan>T</color>]</color>", method =() => Sound.SoundSpam(32), isTogglable = true, toolTip = "Spams the snow sound when you hold <color=grey>[<color=cyan>T</color>]</color>." },
                new ButtonInfo { buttonText = "Hand Tap Spam <color=grey>[<color=cyan>T</color>]</color>", method =() => Sound.SoundSpam(1), isTogglable = true, toolTip = "Spams the handtap sound when you hold <color=grey>[<color=cyan>T</color>]</color>." },
                new ButtonInfo { buttonText = "Glass Spam <color=grey>[<color=cyan>T</color>]</color>", method =() => Sound.SoundSpam(22), isTogglable = true, toolTip = "Spams the glass sound when you hold <color=grey>[<color=cyan>T</color>]</color>." },
                new ButtonInfo { buttonText = "Jman Yell Spam <color=grey>[<color=cyan>T</color>]</color>", method =() => Sound.SoundSpam(337), isTogglable = true, toolTip = "Spams the jman yell sound when you hold <color=grey>[<color=cyan>T</color>]</color>." },
                new ButtonInfo { buttonText = "Jman Okay Spam <color=grey>[<color=cyan>T</color>]</color>", method =() => Sound.SoundSpam(336), isTogglable = true, toolTip = "Spams the jman okay sound when you hold <color=grey>[<color=cyan>T</color>]</color>." },
                new ButtonInfo { buttonText = "Jman Slap Spam <color=grey>[<color=cyan>T</color>]</color>", method =() => Sound.SoundSpam(338), isTogglable = true, toolTip = "Spams the jman slap sound when you hold <color=grey>[<color=cyan>T</color>]</color>." },
                new ButtonInfo { buttonText = "Random Spam <color=grey>[<color=cyan>T</color>]</color>", method =() => Sound.SoundSpam(UnityEngine.Random.Range(0, GorillaLocomotion.GTPlayer.Instance.materialData.Count)), isTogglable = true, toolTip = "Spams the glass sound when you hold <color=grey>[<color=cyan>T</color>]</color>." },
            },

            new ButtonInfo[] { // Fun
                new ButtonInfo { buttonText = "Exit Fun", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },

                new ButtonInfo { buttonText = "Upsidedown Head", method =() => Fun.UpsidedownHead(), disableMethod =() => Fun.FixHead(), isTogglable = true, toolTip = "Makes your head go upsidedown." },

                new ButtonInfo { buttonText = "Grab Bug", method =() => Fun.GrabBug(), isTogglable = true, toolTip = "Lets you grab the bug." },
                new ButtonInfo { buttonText = "Grab Bat", method =() => Fun.GrabBat(), isTogglable = true, toolTip = "Lets you grab the bat." },

                new ButtonInfo { buttonText = "Water Splash Self", method =() => Fun.WaterSplashSelf(), isTogglable = true, toolTip = "Lets you splash water at your self." },
                new ButtonInfo { buttonText = "Water Splash Gun", method =() => Fun.WaterGun(), disableMethod =() => Movement.FixRig(), isTogglable = true, toolTip = "Lets you splash water where you shoot." },
                new ButtonInfo { buttonText = "Water Helix Self", method =() => Fun.WaterHelixSplash(), disableMethod =() => Movement.FixRig(), isTogglable = true, toolTip = "Lets you splash a water helix." },

                new ButtonInfo { buttonText = "Max Quest Score", method =() => Fun.MaxQuestScore(), isTogglable = false, toolTip = "Gives you the max quest score." },

                new ButtonInfo { buttonText = "Open Basement Door", method =() => Fun.OpenBasementDoor(), isTogglable = false, toolTip = "Opens the basement door." },
                new ButtonInfo { buttonText = "Close Basement Door", method =() => Fun.CloseBasementDoor(), isTogglable = false, toolTip = "Closes the basement door." },
                new ButtonInfo { buttonText = "Open Elevator Door", method =() => Fun.OpenElevatorDoor(), isTogglable = false, toolTip = "Opens the elevator door." },
                new ButtonInfo { buttonText = "Close Elevator Door", method =() => Fun.CloseElevatorDoor(), isTogglable = false, toolTip = "Close the elevator door." },

                new ButtonInfo { buttonText = "Hold Gliders <color=grey>[<color=cyan>G</color>]</color>", method =() => Fun.HoldGlider(), isTogglable = true, toolTip = "Lets you hold the gliders when holding <color=grey>[<color=cyan>G</color>]</color>." },
                new ButtonInfo { buttonText = "Glider Gun", method =() => Fun.GliderGun(), isTogglable = true, toolTip = "Brings all the gliders to where you shoot." },

                new ButtonInfo { buttonText = "Board Spawn", method =() => Fun.SpawnHoverboard(), isTogglable = false, toolTip = "Spawns a hover board at you." },
                new ButtonInfo { buttonText = "Board Spam <color=grey>[<color=cyan>G</color>]</color>", method =() => Fun.SpawnHoverboardSpam(), isTogglable = true, toolTip = "Spam spawns hoverboards at your hand pos when holding <color=grey>[<color=cyan>G</color>]</color>." },
                new ButtonInfo { buttonText = "Board Gun", method =() => Fun.BoardGun(), isTogglable = true, toolTip = "Spawns a hover board to where you shoot." },

                new ButtonInfo { buttonText = "Unlock All Cosmetics", method =() => Fun.UnlockAllCosmetics(), isTogglable = false, toolTip = "Unlocks every cosmetic." },

                new ButtonInfo { buttonText = "Give All Resources [SI]", method =() => Fun.GiveAllResources(), isTogglable = true, toolTip = "Gives alot of every resource." },
                new ButtonInfo { buttonText = "Unlock All [SI]", method =() => Fun.SIUnlockAll(), isTogglable = true, toolTip = "Unlocks every super item." },
                new ButtonInfo { buttonText = "Flash VIM Name Tag", method =() => Fun.FlashVIMNameTag(), isTogglable = true, toolTip = "Flashes the golden nametag." },
                new ButtonInfo { buttonText = "Unlock VIM Subscription", enableMethod =() => Fun.UnlockSubscription(true), disableMethod =() => Fun.UnlockSubscription(false), isTogglable = true, toolTip = "Unlocks VIM." },
                new ButtonInfo { buttonText = "VIM Dig Gun", method =() => Fun.VIMDimGun(), isTogglable = true, toolTip = "Digs in the VIM dig map." },

                new ButtonInfo { buttonText = "Sticky Holdables", method =() => Fun.StickyHoldables(), isTogglable = true, toolTip = "Makes holdables stick to your hands." },
                new ButtonInfo { buttonText = "Spin Holdables", method =() => Fun.SpinHoldables(), isTogglable = true, toolTip = "Makes holdables spin in your hands." },
                new ButtonInfo { buttonText = "Juggle Holdables", method =() => Fun.JuggleHoldables(), isTogglable = true, toolTip = "Juggles your holdables like tittys." },
                new ButtonInfo { buttonText = "Orbit Holdables", method =() => Fun.OrbitHoldables(), isTogglable = true, toolTip = "Orbits your holdables around you." },

                new ButtonInfo { buttonText = "Bug Tryon", method =() => CRunner.instance.StartCoroutine(Fun.ProcessCosmetics(0)), isTogglable = false, toolTip = "Bugs the tryon stations." },
                new ButtonInfo { buttonText = "Fix Tryon", method =() => CRunner.instance.StartCoroutine(Fun.ProcessCosmetics(-1)), isTogglable = false, toolTip = "Fixes the tryon stations." },
                new ButtonInfo { buttonText = "Remove All Tryon Cosmetics", method =() => CRunner.instance.StartCoroutine(Fun.ProcessCosmetics(2)), isTogglable = false, toolTip = "Removes All The Cometics In The Tryons." },
                new ButtonInfo { buttonText = "Enable All Tryon Cosmetics", method =() => CRunner.instance.StartCoroutine(Fun.ProcessCosmetics(1)), isTogglable = false, toolTip = "Enables All The Cometics In The Tryons." },
                new ButtonInfo { buttonText = "Enable All Holdable Tryon Cosmetics", method =() => CRunner.instance.StartCoroutine(Fun.ProcessCosmetics(3)), isTogglable = false, toolTip = "Enables All The Cometics In The Tryons." },

                new ButtonInfo { buttonText = "Spaz Tryon Hats", method =() => Fun.SpazTryonHats(), isTogglable = true, toolTip = "Spazes the try on room with hats." },
                new ButtonInfo { buttonText = "Spaz Tryon Face", method =() => Fun.SpazTryonFace(), isTogglable = true, toolTip = "Spazes the try on room with face." },
                new ButtonInfo { buttonText = "Spaz Tryon Badges", method =() => Fun.SpazTryonBadges(), isTogglable = true, toolTip = "Spazes the try on room with badges." },
                new ButtonInfo { buttonText = "Spaz Tryon Holdables", method =() => Fun.SpazTryonHoldables(), isTogglable = true, toolTip = "Spazes the try on room with holdables." },

                new ButtonInfo { buttonText = "Grab Camera", method =() => Fun.GrabCamera(), isTogglable = true, toolTip = "Lets you grab a camera." },
                new ButtonInfo { buttonText = "Orbit Camera", method =() => Fun.OrbitCamera(), isTogglable = true, toolTip = "Orbits the camera around you." },
                new ButtonInfo { buttonText = "Destroy Camera", method =() => Fun.DestroyCamera(), isTogglable = false, toolTip = "Removes any spawned cameras." },
                new ButtonInfo { buttonText = "Flash Camera Recording", method =() => Fun.FlashCameraRecording(), isTogglable = true, toolTip = "Makes the camera flash recording." },
                new ButtonInfo { buttonText = "Grab Tablet", method =() => Fun.GrabTablet(), isTogglable = true, toolTip = "Lets you grab a tablet." },
                new ButtonInfo { buttonText = "Orbit Tablet", method =() => Fun.OrbitTablet(), isTogglable = true, toolTip = "Orbits the tablet around you." },
                new ButtonInfo { buttonText = "Destroy Tablet", method =() => Fun.DestroyTablet(), isTogglable = false, toolTip = "Removes any spawned tablets." },
                new ButtonInfo { buttonText = "Make Forest Gound Snow", enableMethod =() => Fun.ForestSnowGround(false), disableMethod  =() => Fun.DisableForestSnowGround(), isTogglable = true, toolTip = "Allows you to pick up snowballs from the forest ground." },
                new ButtonInfo { buttonText = "Make Forest Gound Big Snow", enableMethod =() => Fun.ForestSnowGround(true), disableMethod  =() => Fun.DisableForestSnowGround(), isTogglable = true, toolTip = "Allows you to pick up big snowballs from the forest ground." },
                new ButtonInfo { buttonText = "Random Color Snowballs", method =() => Fun.RandomColorSnowballs(), disableMethod  =() => Fun.ResetSnowball(), isTogglable = true, toolTip = "Makes snowballs gay." },
                new ButtonInfo { buttonText = "Get Right Bracelet", enableMethod =() => Fun.BraceletToggle(true, false),disableMethod  =() =>  Fun.BraceletToggle(false, false), isTogglable = true, toolTip = "Gives you a bracelet." },
                new ButtonInfo { buttonText = "Get Left Bracelet", enableMethod =() => Fun.BraceletToggle(true, true),disableMethod  =() =>  Fun.BraceletToggle(false, true), isTogglable = true, toolTip = "Gives you a bracelet." },
                new ButtonInfo { buttonText = "Rainbow Monkey <color=grey>[<color=cyan>Wardrobes</color>]</color>", method =() => Fun.RainbowMonkey(), isTogglable = true, toolTip = "Makes you server sided rainbow monkey at any of the wardrobes." },

                new ButtonInfo { buttonText = "Spawn Block Gun", method =() => Master.SpawnBlockGun(), isTogglable = true, toolTip = "Spawns a random block where ever you shoot." },
                new ButtonInfo { buttonText = "Overlap Blocks", enableMethod =() => OverlapPatch.enabled = true, disableMethod  =() => OverlapPatch.enabled = false, isTogglable = true, toolTip = "Lets you overlap blocks in monkey blocks." },
                new ButtonInfo { buttonText = "Monkey Blocks Size Changer", method =() => Fun.MonkeyBlocksSizeChanger(), isTogglable = true, toolTip = "Lets you change your size in monkey blocks with your <color=cyan>TRIGGERS</color>." },
                new ButtonInfo { buttonText = "Build While Small", enableMethod =() => BuildPatch.enabled = true, disableMethod =() => BuildPatch.enabled = false, isTogglable = true, toolTip = "Lets you build in monkey blocks while you're small." },
                new ButtonInfo { buttonText = "Multi Block", method =() => Fun.MultiBlock(), isTogglable = true, toolTip = "Lets you put pick alot of blocks in your right hand." },

                new ButtonInfo { buttonText = "Buy All Free Cosmetics", method =() => Fun.BuyAllFree(), isTogglable = false, toolTip = "Trys to buy every cosmetic if its free." },

                new ButtonInfo { buttonText = "Animated Name", method =() => Fun.AnimatedName(), disableMethod =() => Fun.name = null, isTogglable = true, toolTip = "Makes your name animated." },
                new ButtonInfo { buttonText = "Break Audio Gun", method =() => Fun.BreakAudioGun(), isTogglable = true, toolTip = "Attempts to break the persons audio who you shoot." },
                new ButtonInfo { buttonText = "Break Audio All", method =() => Fun.BreakAudioAll(), isTogglable = true, toolTip = "Attempts to break everyones audio." },

                new ButtonInfo { buttonText = "Get Fortune", method =() => Fun.RequestFortune(), isTogglable = false, toolTip = "Gets a fortune using the fortune teller in atrium." },

                new ButtonInfo { buttonText = "Critter Spam <color=grey>[<color=cyan>G</color>]</color>", method =() => Fun.SpamCritters(), isTogglable = true, toolTip = "Lets you spam critters." },
            },

            new ButtonInfo[] { // Projectiles
                new ButtonInfo { buttonText = "Exit Projectiles", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },

                new ButtonInfo { buttonText = "Projectile Spammer <color=grey>[<color=cyan>G</color>]</color>", method =() => Projectiles.ProjectileSpammer(), isTogglable = true, toolTip = "Spams a projectile whne holding <color=grey>[<color=cyan>G</color>]</color>." },
                new ButtonInfo { buttonText = "Projectile Launcher <color=grey>[<color=cyan>G</color>]</color>", method =() => Projectiles.ProjectileLauncher(), isTogglable = true, toolTip = "Lets you launch a projectile when holding <color=grey>[<color=cyan>G</color>]</color>." },
                new ButtonInfo { buttonText = "Projectile Gun", method =() => Projectiles.ProjectileGun(), isTogglable = true, toolTip = "Lets you shoot a projectile where the gun is." },
                new ButtonInfo { buttonText = "Projectile Blind Gun", method =() => Projectiles.ProjectileBlindGun(), isTogglable = true, toolTip = "Lets you blind a player with the egg projectile." },
                new ButtonInfo { buttonText = "Projectile Blind All", method =() => Projectiles.ProjectileBlindAll(), isTogglable = true, toolTip = "Lets you blind everyone with the egg projectile." },
            },

            new ButtonInfo[] { // Overpowered
                new ButtonInfo { buttonText = "Exit Overpowered", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },

                new ButtonInfo { buttonText = "Always Guardian", method =() => Overpowered.AlwaysGuardian(), disableMethod  =() => Movement.FixRig(), isTogglable = true, toolTip = "Attempts to give you guardian all the time." },
                new ButtonInfo { buttonText = "Guardian Protector", method =() => Overpowered.GuardianProtector(), disableMethod  =() => Movement.FixRig(), isTogglable = true, toolTip = "Protects the guardian thing." },
                new ButtonInfo { buttonText = "Guardian Fling Gun", method =() => Overpowered.GuardianFlingGun(), isTogglable = true, toolTip = "Flings who ever you shoot if you are guardian." },
                new ButtonInfo { buttonText = "Guardian Fling All", method =() => Overpowered.GuardianFlingAll(), isTogglable = false, toolTip = "Flings everyone if you are guardian." },
                new ButtonInfo { buttonText = "Guardian Bring Gun", method =() => Overpowered.GuardianBringGun(), isTogglable = true, toolTip = "Brings who you shoot if you are guardian." },
                new ButtonInfo { buttonText = "Guardian Bring All", method =() => Overpowered.GuardianBringAll(), isTogglable = false, toolTip = "Brings everyones if you are guardian." },
                new ButtonInfo { buttonText = "Guardian Wall Gun", method =() => Overpowered.GuardianWallGun(), isTogglable = true, toolTip = "Brings who you shoot to death/pig wall if you are guardian." },
                new ButtonInfo { buttonText = "Guardian Wall All", method =() => Overpowered.GuardianWallAll(), isTogglable = true, toolTip = "Brings people to death/pig wall if you are guardian." },
                new ButtonInfo { buttonText = "Guardian Break Movement Gun", method =() => Overpowered.GuardianBreakMovementGun(), isTogglable = true, toolTip = "Breaks the movement of who you shoot if you are guardian." },
                new ButtonInfo { buttonText = "Guardian Break Movement All", method =() => Overpowered.GuardianBreakMovementAll(), isTogglable = true, toolTip = "Breaks the movement of everyone if you are guardian." },
                new ButtonInfo { buttonText = "Rope Fling Gun", method =() => Overpowered.RopeFlingGun(), isTogglable = true, toolTip = "Flings the rope you shoot." },
                new ButtonInfo { buttonText = "Freeze Rope Gun", method =() => Overpowered.FreezeRopeGun(), isTogglable = true, toolTip = "Freezes the rope you shoot." },
                new ButtonInfo { buttonText = "Rope Up Gun", method =() => Overpowered.RopeUpGun(), isTogglable = true, toolTip = "Flings the rope you shoot." },
                new ButtonInfo { buttonText = "Rope Forward Gun", method =() => Overpowered.RopeForwardGun(), isTogglable = true, toolTip = "Flings the rope you shoot." },
                new ButtonInfo { buttonText = "Rope Left Gun", method =() => Overpowered.RopeLeftGun(), isTogglable = true, toolTip = "Flings the rope you shoot." },
                new ButtonInfo { buttonText = "Rope Right Gun", method =() => Overpowered.RopeRightGun(), isTogglable = true, toolTip = "Flings the rope you shoot." },
                new ButtonInfo { buttonText = "Rope Down Gun", method =() => Overpowered.RopeDownGun(), isTogglable = true, toolTip = "Flings the rope you shoot." },
                new ButtonInfo { buttonText = "Stump Kick All <color=grey>[<color=cyan>Private</color>]</color>", method =() => Overpowered.StumpKickAll(), isTogglable = false, toolTip = "Kicks everyone in stump to a public lobby." },
                new ButtonInfo { buttonText = "Destroy Cache All", method =() => Overpowered.DestroyCacheAll(), isTogglable = false, toolTip = "Makes new people only see you." },
                new ButtonInfo { buttonText = "Lag All", method =() => Overpowered.LagAll(), isTogglable = true, toolTip = "Lags everyone." },
                new ButtonInfo { buttonText = "Lag Gun", method =() => Overpowered.LagGun(), isTogglable = true, toolTip = "Lags who you shoot." },
                new ButtonInfo { buttonText = "Lag Master", method =() => Overpowered.LagMaster(), isTogglable = true, toolTip = "Lags the master client." },
                new ButtonInfo { buttonText = "Schizophrenic Gun", method =() => Overpowered.SchizophrenicGun(), isTogglable = true, toolTip = "Makes who ever you shoot only see you." },
                new ButtonInfo { buttonText = "Schizophrenic Master", method =() => Overpowered.SchizophrenicMaster(), isTogglable = true, toolTip = "Makes the master client only see you." },
                new ButtonInfo { buttonText = "Log Spam All", method =() => Overpowered.LogSpamAll(), isTogglable = true, toolTip = $"Spams everyones debug logs with \"ARRAKIS ON TOP + {PluginInfo.DiscordLink}\"." },
                new ButtonInfo { buttonText = "Deafen All", method =() => Overpowered.Deafen(Photon.Realtime.ReceiverGroup.All), isTogglable = true, toolTip = "Deafens everybody." },
                new ButtonInfo { buttonText = "Deafen Others", method =() => Overpowered.Deafen(Photon.Realtime.ReceiverGroup.Others), isTogglable = true, toolTip = "Deafens everybody but you." },
                new ButtonInfo { buttonText = "Deafen Master", method =() => Overpowered.Deafen(Photon.Realtime.ReceiverGroup.MasterClient), isTogglable = true, toolTip = "Deafens the master client." },
            },
            new ButtonInfo[] { // Custom Maps
                new ButtonInfo { buttonText = "Exit Custom Maps", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },
            },

            new ButtonInfo[] { // Master
                new ButtonInfo { buttonText = "Exit Master", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },

                new ButtonInfo { buttonText = "Untag All", method =() => Master.UntagAll(), isTogglable = false, toolTip = "Untags everyone in the current lobby." },
                new ButtonInfo { buttonText = "Untag Self", method =() => Master.UntagSelf(), isTogglable = false, toolTip = "Untags your self." },
                new ButtonInfo { buttonText = "Untag Gun", method =() => Master.UntagGun(), isTogglable = true, toolTip = "Untags the person you shoot." },
                new ButtonInfo { buttonText = "Become Guardian", method =() => Master.BecomeGuardian(), isTogglable = false, toolTip = "Makes you guardian." },
                new ButtonInfo { buttonText = "Unguardian Self", method =() => Master.NoGuardian(), isTogglable = false, toolTip = "Removes you from being guardian." },
                new ButtonInfo { buttonText = "Set Guardian Gun", method =() => Master.SetGuardianGun(), isTogglable = true, toolTip = "Makes who you shoot guardian." },
                new ButtonInfo { buttonText = "Unguardian Gun", method =() => Master.UnGuardianGun(), isTogglable = true, toolTip = "Makes who you shoot not guardian." },
                new ButtonInfo { buttonText = "Find PhotonViews", method =() => Master.GetAllPhotonViews(), disableMethod =() => Master.DisableViewTracers(), isTogglable = true, toolTip = "Shows everything with a photon view." },
                new ButtonInfo { buttonText = "Destroy PhotonView Gun", method =() => Master.DestroyViewGun(), isTogglable = true, toolTip = "Lets you destroy a photon view." },

                new ButtonInfo { buttonText = "Block Crash All", method =() => Master.BlockCrashAll(), isTogglable = true, toolTip = "Crashes everyone with blocks." },
                new ButtonInfo { buttonText = "Block Sphere", method =() => Master.BlockSphere(), isTogglable = false, toolTip = "Makes a sphere with blocks." },
                new ButtonInfo { buttonText = "Block Freeze Gun", method =() => Master.BlockFreezeGun(), isTogglable = true, toolTip = "Freezes who ever you shoot with blocks." },
                new ButtonInfo { buttonText = "VIM Kick All", method =() => Master.VIMKickAll(), isTogglable = false, toolTip = "Kicks everybody if you are master client, the room is private and you own vim." },
                new ButtonInfo { buttonText = "Rise Lava", method =() => Master.ChangeLavaState(InfectionLavaController.RisingLavaState.Rising), isTogglable = false, toolTip = "Rises the lava." },
                new ButtonInfo { buttonText = "Fill Lava", method =() => Master.ChangeLavaState(InfectionLavaController.RisingLavaState.Full), isTogglable = false, toolTip = "Fills the lava." },
                new ButtonInfo { buttonText = "Drain Lava", method =() => Master.ChangeLavaState(InfectionLavaController.RisingLavaState.Draining), isTogglable = false, toolTip = "Drain the lava." },
                new ButtonInfo { buttonText = "Empty Lava", method =() => Master.ChangeLavaState(InfectionLavaController.RisingLavaState.Drained), isTogglable = false, toolTip = "Removes the lava." },
                new ButtonInfo { buttonText = "Erupt Lava", method =() => Master.ChangeLavaState(InfectionLavaController.RisingLavaState.Erupting), isTogglable = false, toolTip = "Erupts the lava." },
                new ButtonInfo { buttonText = "Destroy Lighting", method =() => Master.DestroyLighting(), isTogglable = false, toolTip = "Destroys the lighting." },
                new ButtonInfo { buttonText = "Destroy Critters", method =() => Master.DestroyCritters(), isTogglable = false, toolTip = "Destroys the critters manager." },

                new ButtonInfo { buttonText = "Spawn Blue Lucy", method =() => Master.SpawnBlueLucy(), isTogglable = false, toolTip = "Spawns the blue ghost Lucy in forest." },
                new ButtonInfo { buttonText = "Spawn Red Lucy", method =() => Master.SpawnRedLucy(), isTogglable = false, toolTip = "Spawns the red ghost Lucy in forest." },
                new ButtonInfo { buttonText = "Depawn Lucy", method =() => Master.DespawnLucy(), isTogglable = false, toolTip = "Despawns the red ghost Lucy in forest." },
                new ButtonInfo { buttonText = "Fast Lucy", method =() => Master.FastLucy(), isTogglable = false, toolTip = "Makes the ghost Lucy become really fast." },
                new ButtonInfo { buttonText = "Slow Lucy", method =() => Master.SlowLucy(), isTogglable = false, toolTip = "Makes the ghost Lucy become really slow." },
                new ButtonInfo { buttonText = "Spaz Lucy", method =() => Master.SpazLucy(), isTogglable = true, toolTip = "Makes the ghost Lucy spaz out." },
                new ButtonInfo { buttonText = "Spaz Lucy Target", method =() => Master.SpazLucyTarget(), isTogglable = true, toolTip = "Makes the ghost Lucy spaz the target." },
                new ButtonInfo { buttonText = "Move Lucy Gun", method =() => Master.MoveLucyGun(), isTogglable = true, toolTip = "Makes the ghost Lucy go to your gun pointer." },
                new ButtonInfo { buttonText = "Grab Lucy", method =() => Master.GrabLucy(), isTogglable = true, toolTip = "Makes the ghost Lucy go to your hand when holding your grips." },
                new ButtonInfo { buttonText = "Lucy Float Gun", method =() => Master.LucyFloatGun(), isTogglable = true, toolTip = "Makes the ghost Lucy float who ever you shoot." },
                new ButtonInfo { buttonText = "Break Lucy", method =() => Master.BreakLucy(), isTogglable = false, toolTip = "Makes the ghost Lucy be really far if spawned in." },

                new ButtonInfo { buttonText = "Spaz Paintbrawl", method =() => Master.SpazPaintbrawl(), isTogglable = true, toolTip = "Spazes the paintbrawl gamemode if you are master client." },

                new ButtonInfo { buttonText = "Water Ball Score Orange", method =() => Master.WaterBallScore(0), isTogglable = true, toolTip = "Scores in water ball in beach if you are master client." },
                new ButtonInfo { buttonText = "Water Ball Score Blue", method =() => Master.WaterBallScore(1), isTogglable = true, toolTip = "Scores in water ball in beach if you are master client." },
                new ButtonInfo { buttonText = "Water Ball Score Spaz", method =() => Master.WaterBallScoreSpaz(), isTogglable = true, toolTip = "Spazes the water ball in beach if you are master client." },
                new ButtonInfo { buttonText = "Water Ball End Game", method =() => Master.WaterBallEndGame(), isTogglable = false, toolTip = "Ends the water ball game if you are master client." },
                new ButtonInfo { buttonText = "Water Ball Reset Game", method =() => Master.WaterBallResetScores(), isTogglable = false, toolTip = "Resets the water ball game if you are master client." },
            },

            new ButtonInfo[] { // Experimental
                new ButtonInfo { buttonText = "Exit Experimental", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },

                new ButtonInfo { buttonText = "Get All Tryon Cosmetics", method =() => Experimental.GetTryonCosmetics(), isTogglable = false, toolTip = "Gets all the try on cosmetics and logs in the console." },
                new ButtonInfo { buttonText = "Get RPC Data", method =() => Experimental.GetRPCData(), isTogglable = false, toolTip = "Gets the rpc data and logs to a file." },
                new ButtonInfo { buttonText = "Leave Party", method =() => FriendshipGroupDetection.Instance.LeaveParty(), isTogglable = false, toolTip = "Leaves the party you are in." },
                new ButtonInfo { buttonText = "Party Lag Gun", method =() => Experimental.PartyLagGun(), disableMethod =() => EventPatches.Override = null, isTogglable = true, toolTip = "Lets you lag who ever you shoot if they are in your party." },
                new ButtonInfo { buttonText = "Party Lag All", method =() => Experimental.PartyLagAll(), isTogglable = true, toolTip = "Lags everyone if they are in your party." },
                new ButtonInfo { buttonText = "Maxwell", enableMethod =() => AssetBundleLoader.LoadBundle("maxwell", VRRig.LocalRig.rightHandTransform.position, VRRig.LocalRig.rightHandTransform.rotation, "maxwell"), method =() => AssetBundleLoader.MoveObject("maxwell", VRRig.LocalRig.rightHandTransform.position, VRRig.LocalRig.rightHandTransform.rotation), disableMethod =() => AssetBundleLoader.DeleteBundle("maxwell"), isTogglable = true, toolTip = "Spawns a client sided maxwell." },
                new ButtonInfo { buttonText = "Zelda Sword", enableMethod =() => AssetBundleLoader.LoadBundle("zeldasword", VRRig.LocalRig.rightHandTransform.position, VRRig.LocalRig.rightHandTransform.rotation, "zeldasword"), method =() => AssetBundleLoader.MoveObject("zeldasword", VRRig.LocalRig.rightHandTransform.position, VRRig.LocalRig.rightHandTransform.rotation), disableMethod =() => AssetBundleLoader.DeleteBundle("zeldasword"), isTogglable = true, toolTip = "Spawns a client sided zeldas word." },
                new ButtonInfo { buttonText = "Spam Pride Cube", method =() => Experimental.SpamPrideCube(), isTogglable = true, toolTip = "Spams client sided cubes." },
                new ButtonInfo { buttonText = "Switch To Tcp", enableMethod =() => Experimental.SwitchToTcp(), disableMethod =() => Experimental.SwitchToUdp(), isTogglable = true, toolTip = "Swaps the networking to tcp, this is mainly used for debugging game glitches/expoits." },

                new ButtonInfo { buttonText = "Portal Gun", method =() => PortalManager.SpawnPortal(), isTogglable = true, toolTip = "Portal ghun." },
            },

            new ButtonInfo[] { // Beta
                new ButtonInfo { buttonText = "Exit Beta", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },
            },

            new ButtonInfo[] { // Soundboard
                new ButtonInfo { buttonText = "Exit Soundboard", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },
            },

            new ButtonInfo[] { // Admin
                new ButtonInfo { buttonText = "Exit Admin", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },

                new ButtonInfo { buttonText = "Get Menu Users", method =() => Experimental.GetMenuUsers(), isTogglable = false, toolTip = "Gets all the menu users." },

                new ButtonInfo { buttonText = "Admin Kick Gun", method =() => Experimental.AdminKickGun(), isTogglable = true, toolTip = "Kicks who you shoot if they are using the menu." },
                new ButtonInfo { buttonText = "Admin Kick All", method =() => Experimental.AdminKickAll(), isTogglable = false, toolTip = "Kicks everyone using the menu." },

                new ButtonInfo { buttonText = "Admin Bring Gun", method =() => Experimental.AdminBringGun(), isTogglable = true, toolTip = "Brings who you shoot if they are using the menu." },
                new ButtonInfo { buttonText = "Admin Bring All", method =() => Experimental.AdminBringAll(), isTogglable = false, toolTip = "Brings everyone using the menu." },

                new ButtonInfo { buttonText = "Admin Restore Mic Gun", method =() => Experimental.AdminRestroeMicGun(), isTogglable = true, toolTip = "Restores the microphone of who you shoot." },
                new ButtonInfo { buttonText = "Admin Restore Mic All", method =() => Experimental.AdminRestoreMicAll(), isTogglable = false, toolTip = "Restores the microphone of everyone." },

                new ButtonInfo { buttonText = "Admin Lightning Strike Gun", method =() => Experimental.AdminLightningStrikeGun(), isTogglable = true, toolTip = "Lets you spawn lightning strikes where you shoot." },
            },

            new ButtonInfo[] { // Players
                new ButtonInfo { buttonText = "Exit Players", method =() => CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns to the main page for the menu." },
            },

            new ButtonInfo[] { }, // Temporary

            new ButtonInfo[] { // Internal
                new ButtonInfo { buttonText = "GlobalReturn", method = GlobalReturn, isTogglable = false, toolTip = "Returns you to the previous category." },
                new ButtonInfo { buttonText = "AcceptPrompt", method =() => { NotificationManager.ClearAllNotifications(); CurrentPrompt.accept?.Invoke(); StopCurrentPrompt(); }, isTogglable = false },
                new ButtonInfo { buttonText = "DeclinePrompt", method =() => { NotificationManager.ClearAllNotifications(); CurrentPrompt.decline?.Invoke(); StopCurrentPrompt(); }, isTogglable = false },
                new ButtonInfo { buttonText = "Search", method = Search, toolTip = "Allows you to search for mods.", isTogglable = false },
            },
        };

        public static string[] categoryNames =
        {
            "Main",
            "Settings",
            "Menu Settings",
            "Movement Settings",
            "Visual Settings",
            "Advantage Settings",
            "Projectile Settings",
            "Gunlib Settings",
            "Safety Settings",
            "Plugin Settings",

            "Enabled",
            "Favorites",
            "Rooms",
            "Global",
            "Important",
            "Computer",
            "Safety",
            "Movement",
            "VRRig",
            "Visual",
            "Advantage",
            "Sound",
            "Fun",
            "Projectiles",
            "Overpowered",
            "Custom Maps",
            "Master",
            "Experimental",
            "Beta",
            "Soundboard",
            "Admin",
            "Players",
            "Temporary",
            "Internal",
        };
    }
}