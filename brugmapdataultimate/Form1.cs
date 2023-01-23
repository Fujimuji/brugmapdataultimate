using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using brugmapdataultimate.Properties;
using Newtonsoft.Json;
using Octokit;
using static brugmapdataultimate.Form1;
using FileMode = System.IO.FileMode;

namespace brugmapdataultimate;

public partial class Form1 : Form
{
    #region copy
    public Form1()
    {
        InitializeComponent();
        CultureInfo cultureInfo = CultureInfo.GetCultureInfo("en-US");
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
        effTypeComboBox.SelectedIndex = 0;
        missTypeComboBox.SelectedIndex = 0;
        lvlIconComboBox.SelectedIndex = 0;
        lvlColorComboBox.SelectedIndex = 0;
        mapTreeView.SelectedNode = mapTreeView.Nodes[0];
        missSettingsGroupBox.Location = new Point(368, 26);
        effSettingsGroupBox.Location = new Point(368, 26);
        cpSettingsGroupBox.Location = new Point(368, 96);
    }

    public enum CheckpointType
    {
        LevelSelector,
        LevelStart,
        LevelEnd,
        Normal
    }

    public enum EffectType
    {
        Time = 0,
        Death = 1,
        Ability = 2,
        Permeation = 3,
        Safepoint = 4,
        EntryPortal = 5,
        ExitPortal = 6
    }

    public enum MissionType
    {
        NoRocketPunch = 2,
        NoSlam = 3,
        NoPowerblock = 5,
        Stalless = 7,
        Spin = 13,
        RocketPunchFirst = 17,
        SlamFirst = 19,
        PowerblockFirst = 23,
        UpwardsDiag = 29,
        DownwardsDiag = 31,
        PunchBounce = 37
    }

    public class Map
    {
        public Map()
        {

            levelSelectorCP = new Checkpoint();
            Levels = new List<Level>();
        }

        public string SelectedMap { get; set; } = "Eichenwalde";
        public string Type { get; set; } = "Hybrid";
        public string TopLeftInfo { get; set; } = "<CUSTOMIZE IN GLOBAL HUD RULE>";
        public Checkpoint levelSelectorCP { get; set; }
        public List<Level> Levels { get; set; }

        public string GenerateMapData(bool isPanelActivated)
        {
            string genCPposition = "Global.CPposition = Array(";
            string genPrime = "Global.Prime = Array(";
            string genRadVAGobackCP = "Global.Radius_VA_GoBackCP = Array(";
            string genConnections = "Global.Connections = Array(";
            string genMission = "Global.Mission = Array(";
            string genHiddenCPTpRadTT = "Global.HiddenCP_TpRad_TT = Array(";
            string genTP = "Global.TP = Array(";
            string genEffect = "Global.Effect = Array(";
            string genAbilityCount = "Global.AbilityCount = Array(";

            genCPposition += $"Vector({levelSelectorCP.Coordinate})";
            genPrime += $"{levelSelectorCP.Prime}";
            genRadVAGobackCP += $"Vector({levelSelectorCP.Radius.Replace(',', '.')},0,-1)";
            genConnections += "0";
            genMission += $"{levelSelectorCP.MissionStringForCP()}";
            genHiddenCPTpRadTT += $"False";
            genTP += $"False";
            genEffect += $"{levelSelectorCP.EffectStringForCP()}";
            genAbilityCount += $"{levelSelectorCP.AbilityCountStringForCP()}";

            int cpcount = 1;
            for (int i = 0; i < Levels.Count; i++)
            {
                for (int i1 = 0; i1 < Levels[i].Checkpoints.Count; i1++)
                {
                    Checkpoint cp = Levels[i].Checkpoints[i1];
                    genCPposition += "," + cp.CPPositionStringForCP();
                    genPrime += "," + cp.Prime;
                    genRadVAGobackCP += "," + $"Vector({cp.Radius.Replace(',', '.')},0," + (cp.Type == CheckpointType.LevelStart ? "0" : (cpcount - 1).ToString()) + ")";
                    genConnections += "," + (cp.Type == CheckpointType.LevelEnd ? 0 : cpcount + 1);
                    genMission += "," + cp.MissionStringForCP();
                    genHiddenCPTpRadTT += "," + cp.HiddenCPTPRadTTStringForCP();
                    genTP += "," + cp.TeleportStringForCP();
                    genEffect += "," + cp.EffectStringForCP();
                    genAbilityCount += "," + cp.AbilityCountStringForCP();
                    cpcount++;
                }

                if (i == Levels.Count - 1)
                {
                    genCPposition += ");";
                    genPrime += ");";
                    genRadVAGobackCP += ");";
                    genConnections += ");";
                    genMission += ");";
                    genHiddenCPTpRadTT += ");";
                    genTP += ");";
                    genEffect += ");";
                    genAbilityCount += ");";
                }
            }

            string wholemapdata = Properties.Resources.playtemplate;
            int firstdataindex = wholemapdata.IndexOf("\"First Data Goes Here\"");
            if (firstdataindex != -1)
            {
                wholemapdata = wholemapdata.Insert(firstdataindex + "\"First Data Goes Here\"".Length, $"\r\n\t\t{genCPposition}\r\n\t\t{genPrime}\r\n\t\t{genRadVAGobackCP}");
            }

            int seconddataindex = wholemapdata.IndexOf("\"Second Data Goes Here\"");
            if (seconddataindex != -1)
            {
                wholemapdata = wholemapdata.Insert(seconddataindex + "\"Second Data Goes Here\"".Length, $"\r\n\t\t{genConnections}\r\n\t\t{genMission}\r\n\t\t{genHiddenCPTpRadTT}");
            }

            int thirddataindex = wholemapdata.IndexOf("\"Third Data Goes Here\"");
            if (thirddataindex != -1)
            {
                wholemapdata = wholemapdata.Insert(thirddataindex + "\"Third Data Goes Here\"".Length, $"\r\n\t\t{genTP}\r\n\t\t{genEffect}\r\n\t\t{genAbilityCount}");
            }

            int lvlnameindex = wholemapdata.IndexOf("\"Customize Level Names\"");
            if (lvlnameindex != -1)
            {
                string lvlnamestring = $"\r\n\t\t\tGlobal.LvlName = Array(Custom String(\"Diverge / Single\"),";
                for (int i = 0; i < Levels.Count; i++)
                {
                    Level lvl = Levels[i];
                    if (i == Levels.Count - 1)
                    {
                        lvlnamestring += $" Custom String(\"{lvl.Name}\"));";
                    }

                    else
                    {
                        lvlnamestring += $" Custom String(\"{lvl.Name}\"),";
                    }
                }
                wholemapdata = wholemapdata.Insert(lvlnameindex + "\"Customize Level Names\"".Length, lvlnamestring);
            }

            int lvlcolorindex = wholemapdata.IndexOf("\"Customize Level Colors\"");
            if (lvlcolorindex != -1)
            {
                string lvlcolorstring = $"\r\n\t\t\tGlobal.LvlColors = Array(Color(Red),";
                for (int i = 0; i < Levels.Count; i++)
                {
                    Level lvl = Levels[i];
                    if (i == Levels.Count - 1)
                    {
                        lvlcolorstring += $" {lvl.Color});";
                    }

                    else
                    {
                        lvlcolorstring += $" {lvl.Color},";
                    }
                }
                wholemapdata = wholemapdata.Insert(lvlcolorindex + "\"Customize Level Colors\"".Length, lvlcolorstring);
            }

            int lvliconindex = wholemapdata.IndexOf("Skip(-2 + Global.LevelCounter * 2);");
            if (lvliconindex != -1)
            {
                string lvliconstring = "";
                for (int i = 0; i < Levels.Count; i++)
                {
                    Level lvl = Levels[i];
                    if (i == Levels.Count - 1)
                    {
                        lvliconstring += $"\r\n\t\t\"{lvl.Name}\"\r\n\t\tCreate Icon(Filtered Array(All Players(All Teams), Current Array Element.CPData[13]),\r\n\t\t\tGlobal.CPposition[Global.Detector1] + Up * 1.750, {lvl.Icon}, Visible To, Global.LvlColors[Global.LevelCounter], True);";
                    }

                    else
                    {
                        lvliconstring += $"\r\n\t\t\"{lvl.Name}\"\r\n\t\tCreate Icon(Filtered Array(All Players(All Teams), Current Array Element.CPData[13]),\r\n\t\t\tGlobal.CPposition[Global.Detector1] + Up * 1.750, {lvl.Icon}, Visible To, Global.LvlColors[Global.LevelCounter], True);" + Environment.NewLine + "\t\tAbort;";
                    }
                }
                wholemapdata = wholemapdata.Insert(lvliconindex + "Skip(-2 + Global.LevelCounter * 2);".Length, lvliconstring);

            }

            int playericonindex = wholemapdata.IndexOf("Skip(Event Player.Level * 2);");
            if (playericonindex != -1)
            {
                string lvlplayericonstring = "";
                lvlplayericonstring += $"\r\n\t\t\"Diverge / Single / Pioneer\"\r\n\t\tCreate Icon(Event Player, Event Player.Local_Pos[Event Player.DelGenElements] + Up * 1.750, Flag, None,\r\n\t\t\tEvent Player.Pioneer ? Color(Orange) : Global.LvlColors[Event Player.Level], True);" + Environment.NewLine + "\t\tAbort;"; ;
                for (int i = 0; i < Levels.Count; i++)
                {
                    Level lvl = Levels[i];
                    if (i == Levels.Count - 1)
                    {
                        lvlplayericonstring += $"\r\n\t\t\"{lvl.Name}\"\r\n\t\tCreate Icon(Event Player, Event Player.Local_Pos[Event Player.DelGenElements] + Up * 1.750, {lvl.Icon}, None,\r\n\t\t\tGlobal.LvlColors[Event Player.Level], True);";
                    }

                    else
                    {
                        lvlplayericonstring += $"\r\n\t\t\"{lvl.Name}\"\r\n\t\tCreate Icon(Event Player, Event Player.Local_Pos[Event Player.DelGenElements] + Up * 1.750, {lvl.Icon}, None,\r\n\t\t\tGlobal.LvlColors[Event Player.Level], True);" + Environment.NewLine + "\t\tAbort;";
                    }
                }

                wholemapdata = wholemapdata.Insert(playericonindex + "Skip(Event Player.Level * 2);".Length, lvlplayericonstring);
            }

            wholemapdata = wholemapdata.Replace("<CUSTOMIZE IN GLOBAL HUD RULE>", TopLeftInfo);
            wholemapdata = wholemapdata.Replace($"\"{Type}\"", SelectedMap + " 0");
            string[] maptypes = { "Assault", "Control", "Escort", "Hybrid", "Skirmish", "Team Deathmatch" };
            wholemapdata = maptypes.Aggregate(wholemapdata, (current, t) => current.Replace($"\"{t}\"", string.Empty));

            if (!isPanelActivated)
            {
                wholemapdata = wholemapdata.Replace("rule(\"display\")", "disabled rule(\"display\")");
            }
            return wholemapdata;
        }
    }

    public class Level
    {
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "Flag";
        public string Color { get; set; } = "Green";
        public List<Checkpoint> Checkpoints { get; set; } = new List<Checkpoint>();
    }

    public class Checkpoint
    {
        public string Coordinate { get; set; } = "";
        public string Prime { get; set; } = "1";
        public string Radius { get; set; } = "2";
        public int GoBackCP { get; set; } = 0;
        public string TeleportRadius { get; set; } = "2";
        public string TeleportCoordinate { get; set; } = "";

        public string PunchCount { get; set; } = "0";
        public string SlamCount { get; set; } = "0";
        public string PowerblockCount { get; set; } = "0";

        public bool EffectLock { get; set; } = false;
        public bool PunchEnabled { get; set; } = false;
        public bool SlamEnabled { get; set; } = false;
        public bool PowerblockEnabled { get; set; } = false;

        public bool isAbilCount { get; set; } = false;

        public List<Effect> Effects { get; set; } = new List<Effect>();

        public List<Mission> Missions { get; set; } = new List<Mission>();

        public CheckpointType Type { get; set; } = CheckpointType.Normal;

        public string CPPositionStringForCP()
        {
            return $"Vector({Coordinate})";
        }

        public string TeleportStringForCP()
        {
            return TeleportCoordinate == "" ? "False" : $"Vector({TeleportCoordinate})";
        }

        public string HiddenCPTPRadTTStringForCP()
        {
            return TeleportCoordinate == "" ? "False" : $"Vector(0,{TeleportRadius},0)";
        }

        public string MissionStringForCP()
        {
            if (!Missions.Any())
            {
                return "True";
            }

            string genMission = "Array(";
            int deflock = 9930;

            int _prime = Missions.Aggregate(1, (current, t) => current * (int)t.Type);

            genMission += _prime.ToString() + ",";

            for (var index = 0; index < Missions.OrderBy(x => x.Type).ToList().Count; index++)
            {
                Mission m = Missions.OrderBy(x => x.Type).ToList()[index];

                if (m.isTimeMission)
                {
                    genMission += index == Missions.OrderBy(x => x.Type).ToList().Count - 1
                        ? m.TimeValue + ")"
                        : m.TimeValue + ",";
                }

                if (m.isLock)
                {
                    genMission += index == Missions.OrderBy(x => x.Type).ToList().Count - 1
                        ? deflock.ToString() + ")"
                        : deflock.ToString() + ",";
                    deflock++;
                }
            }
            return genMission;
        }

        public string EffectStringForCP()
        {
            if (!Effects.Any())
            {
                return "False";
            }

            string genEffect = "Array(";
            for (int i = 0; i < Effects.Count; i++)
            {
                genEffect += i == Effects.Count - 1
                    ? Effects[i].EffectString() + ")"
                    : Effects[i].EffectString() + ",";
            }
            return genEffect;
        }

        public string AbilityCountStringForCP()
        {
            return isAbilCount == false ? "False" : $"Vector({PunchCount},{PowerblockCount},{SlamCount})";
        }
    }

    public class Effect
    {
        public string Coordinate { get; set; } = "";
        public string Radius { get; set; } = "1";
        public string Prime { get; set; } = "1";
        public EffectType Type { get; set; } = EffectType.Time;
        public bool PunchEnabled { get; set; } = false;
        public bool SlamEnabled { get; set; } = false;
        public bool PowerblockEnabled { get; set; } = false;
        public string TimeValue { get; set; } = "0";
        public bool Lightshaft { get; set; } = false;
        public bool Normal { get; set; } = false;
        public bool CDReset { get; set; } = false;
        public bool NoChange { get; set; } = false;

        public string EffectString()
        {
            return Type switch
            {
                EffectType.Time => $"Array(Vector({Coordinate}), {Radius}, 0, {TimeValue})",
                EffectType.Death => $"Array(Vector({Coordinate}), {Radius}, 1, 1)",
                EffectType.Ability => $"Array(Vector({Coordinate}), {Radius}, 2, {Prime})",
                EffectType.Permeation => $"Array(Vector({Coordinate}), {Radius}, 3, {Prime})",
                EffectType.Safepoint => $"Array(Vector({Coordinate}), {Radius}, 4, {Prime})",
                EffectType.EntryPortal => $"Array(Vector({Coordinate}), {Radius}, 5, {Prime})",
                EffectType.ExitPortal => $"Array(Vector({Coordinate}), {Radius}, 6, {Prime})"
            };
        }
    }

    public class Mission
    {
        public MissionType Type { get; set; } = MissionType.Stalless;
        public bool isLock { get; set; } = false;
        public bool isTimeMission { get; set; } = false;
        public string TimeValue { get; set; } = "0";
    }

    public Map map = new Map();
    string pattern = @"(-?\d+(?:\.\d+)?),\s*(-?\d+(?:\.\d+)?),\s*(-?\d+(?:\.\d+)?)";
    const string version = "v1.0.7";

    public int GeneratePrimeForCP()
    {
        return cpAbilGroupBox.Controls.OfType<CheckBox>().Where(c => !c.Checked).Aggregate(1, (current, c) => current * int.Parse(c.Tag.ToString()));
    }

    public int GeneratePrimeForEffect()
    {
        return effAbilGroupBox.Controls.OfType<CheckBox>().Where(c => !c.Checked).Aggregate(1, (current, c) => current * int.Parse(c.Tag.ToString()));
    }

    private void isNoChange_CheckedChanged(object sender, EventArgs e)
    {
        if (isNoChange.Checked)
        {
            foreach (CheckBox c in effAbilGroupBox.Controls.OfType<CheckBox>())
            {
                c.Checked = false;
            }
        }
    }

    public void HideEffectsMissions()
    {
        effSettingsGroupBox.Visible = false;
        missSettingsGroupBox.Visible = false;
    }

    public void EnableCPControls()
    {
        cpSettingsGroupBox.Enabled = true;
        cpSettingsGroupBox.Visible = true;
        isLvlStartCP.Enabled = true;
        isLvlEndCP.Enabled = true;
        isNormalCP.Enabled = true;
        isTeleport.Enabled = true;
        tpRadTxt.Enabled = isTeleport.Checked;
        tpCoordTxt.Enabled = isTeleport.Checked;
    }

    private void effAbilities_CheckedChanged(object sender, EventArgs e)
    {
        var c = (CheckBox)sender;
        if (c.Checked)
        {
            if (isNoChange.Checked)
            {
                isNoChange.Checked = false;
            }
        }

        int checkcount = effAbilGroupBox.Controls.OfType<CheckBox>().Count(z => z.Checked);
        if (checkcount == 0)
        {
            isNoChange.Checked = true;
        }
    }

    public void SelectTheCPtype(CheckpointType type)
    {
        switch (type)
        {
            case CheckpointType.LevelStart:
                isLvlStartCP.Checked = true;
                break;
            case CheckpointType.LevelEnd:
                isLvlEndCP.Checked = true;
                break;
            case CheckpointType.Normal:
                isNormalCP.Checked = true;
                break;
        }
    }

    private void mapTreeView_AfterSelect(object sender, TreeViewEventArgs e)
    {
        if (e.Node?.Text == "Map")
        {
            topleftTxt.Text = map.TopLeftInfo == "<CUSTOMIZE IN GLOBAL HUD RULE>" ? "" : map.TopLeftInfo;
            mapComboBox.SelectedItem = map.SelectedMap;
            if (e.Node.FirstNode?.Text != "Checkpoint 0") //level selector cp hasn't been added
            {
                ClearCPControls();
                HideEffectsMissions();
                generalGroupBox.Visible = true;
                cpSettingsGroupBox.Enabled = true;
                cpSettingsGroupBox.Location = new Point(368, 96);
                cpSettingsGroupBox.Visible = true;
                addCpBtn.Text = "Add Checkpoint";
                isTeleport.Enabled = false;
                isLvlSelector.Enabled = true;
                isLvlSelector.Checked = true;
                isLvlStartCP.Enabled = false;
                lvlGroupBox.Visible = false;
                isLvlEndCP.Enabled = false;
                isNormalCP.Enabled = false;
                return;
            }

            else //level selector cp exists and now can add a level
            {
                generalGroupBox.Visible = true;
                cpSettingsGroupBox.Visible = false;
                HideEffectsMissions();
                lvlGroupBox.Location = new Point(368, 96);
                lvlGroupBox.Visible = true;
                lvlNameTxt.Enabled = true;
                addLvlBtn.Enabled = true;
                addLvlBtn.Text = "Add Level";
                lvlNameTxt.Text = "";
                return;
            }
        }

        if (e.Node?.Text == "Checkpoint 0") //level selector cp selected
        {
            HideEffectsMissions();
            generalGroupBox.Visible = false;
            cpSettingsGroupBox.Location = new Point(368, 26);
            cpSettingsGroupBox.Visible = true;
            addCpBtn.Text = "Edit Checkpoint";
            isLvlSelector.Enabled = true;
            isLvlSelector.Checked = true;
            isLvlStartCP.Enabled = false;
            lvlGroupBox.Visible = false;
            isLvlEndCP.Enabled = false;
            isNormalCP.Enabled = false;
            cpCoordTxt.Text = map.levelSelectorCP.Coordinate;
            cpRadTxt.Value = decimal.Parse(map.levelSelectorCP.Radius);
            punchUpDown.Value = decimal.Parse(map.levelSelectorCP.PunchCount);
            slamUpDown.Value = decimal.Parse(map.levelSelectorCP.SlamCount);
            powerBlockUpDown.Value = decimal.Parse(map.levelSelectorCP.PowerblockCount);
            cpPunchEnabled.Checked = map.levelSelectorCP.PunchEnabled;
            cpSlamEnabled.Checked = map.levelSelectorCP.SlamEnabled;
            cpPowerBlockEnabled.Checked = map.levelSelectorCP.PowerblockEnabled;
            cpPunchEnabled.Enabled = true;
            cpSlamEnabled.Enabled = true;
            cpPowerBlockEnabled.Enabled = true;
            isEffLocked.Checked = map.levelSelectorCP.EffectLock;
            isEffLocked.Enabled = map.levelSelectorCP.Effects.Any(x => x.Type == EffectType.Ability || x.Type == EffectType.Time);
            return;
        }

        if (e.Node.Text.Contains("Level -")) //a level is selected
        {
            ClearCPControls();
            EnableCPControls();
            lvlGroupBox.Location = new Point(368, 26);
            generalGroupBox.Visible = false;
            isLvlSelector.Enabled = false;
            isNormalCP.Checked = true;
            string currentLvlName = mapTreeView.SelectedNode.Tag.ToString();
            var currentlv = map.Levels.First(x => x.Name == currentLvlName);
            lvlIconComboBox.SelectedItem = currentlv.Icon;
            lvlColorComboBox.SelectedItem = currentlv.Color.Split('(', ')')[1];
            bool doesLvlFirstCpExist = map.Levels.First(x => x.Name == currentLvlName).Checkpoints.Any(x => x.Type == CheckpointType.LevelStart);
            bool doesLvlLastCpExist = map.Levels.First(x => x.Name == currentLvlName).Checkpoints.Any(x => x.Type == CheckpointType.LevelEnd);
            if (doesLvlFirstCpExist)
            {
                isLvlStartCP.Checked = false;
                isLvlStartCP.Enabled = false;
            }

            if (!doesLvlFirstCpExist)
            {
                isLvlStartCP.Checked = true;
                isLvlStartCP.Enabled = true;
                isLvlEndCP.Enabled = false;
                isNormalCP.Enabled = false;
                isTeleport.Enabled = true;
                isTeleport.Checked = false;
                lvlGroupBox.Visible = true;
                cpSettingsGroupBox.Location = new Point(368, 94);
                cpSettingsGroupBox.Visible = true;
                HideEffectsMissions();
                addLvlBtn.Text = "Edit Level";
                addLvlBtn.Enabled = true;
                addCpBtn.Text = "Add Checkpoint";
                addCpBtn.Enabled = true;
                lvlNameTxt.Text = e.Node.Tag.ToString();
                return;
            }

            if (doesLvlLastCpExist)
            {
                isLvlEndCP.Checked = false;
                isLvlEndCP.Enabled = false;
                isTeleport.Enabled = true;
                isTeleport.Checked = false;
                isEffLocked.Checked = false;
                isNormalCP.Enabled = true;
            }
            lvlGroupBox.Visible = true;
            cpSettingsGroupBox.Location = new Point(368, 94);
            cpSettingsGroupBox.Visible = true;
            HideEffectsMissions();
            addLvlBtn.Text = "Edit Level";
            addLvlBtn.Enabled = true;
            addCpBtn.Text = "Add Checkpoint";
            addCpBtn.Enabled = true;
            lvlNameTxt.Text = e.Node.Tag.ToString();
            return;
        }

        if (e.Node.Text == "Effects") //effects tree selected
        {
            generalGroupBox.Visible = false;
            effSettingsGroupBox.Visible = true;
            cpSettingsGroupBox.Visible = false;
            effTypeComboBox.Enabled = true;
            lvlGroupBox.Visible = false;
            missSettingsGroupBox.Visible = false;
            addEffBtn.Text = "Add Effect";
            effCoordTxt.Text = "";
            effRadTxt.Value = decimal.Parse("1.1");
            return;
        }

        if (e.Node.Text == "Missions") //missions tree selected
        {
            generalGroupBox.Visible = false;
            addMissBtn.Text = "Add Mission";
            missSettingsGroupBox.Visible = true;
            cpSettingsGroupBox.Visible = false;
            lvlGroupBox.Visible = false;
            effSettingsGroupBox.Visible = false;
            return;
        }

        if (e.Node.Text.Contains("Checkpoint")) //a checkpoint under a level selected
        {
            generalGroupBox.Visible = false;
            isNormalCP.Enabled = true;
            string currentLvlName = mapTreeView.SelectedNode.Parent.Tag.ToString();
            bool doesLvlFirstCpExist = map.Levels.First(x => x.Name == currentLvlName).Checkpoints.Any(x => x.Type == CheckpointType.LevelStart);
            bool doesLvlLastCpExist = map.Levels.First(x => x.Name == currentLvlName).Checkpoints.Any(x => x.Type == CheckpointType.LevelEnd);
            isLvlEndCP.Enabled = !doesLvlLastCpExist;
            isLvlStartCP.Enabled = !doesLvlFirstCpExist;
            isLvlSelector.Enabled = false;
            HideEffectsMissions();
            SelectTheCPtype(map.Levels.First(x => x.Name == currentLvlName).Checkpoints[e.Node.Index].Type);
            cpSettingsGroupBox.Location = new Point(368, 26);
            cpSettingsGroupBox.Visible = true;
            lvlGroupBox.Visible = false;
            addCpBtn.Text = "Edit Checkpoint";
            cpCoordTxt.Text = map.Levels.First(x => x.Name == currentLvlName).Checkpoints[e.Node.Index].Coordinate;
            cpRadTxt.Value = decimal.Parse(map.Levels.First(x => x.Name == currentLvlName).Checkpoints[e.Node.Index].Radius);
            cpPunchEnabled.Checked = map.Levels.First(x => x.Name == currentLvlName).Checkpoints[e.Node.Index].PunchEnabled;
            cpSlamEnabled.Checked = map.Levels.First(x => x.Name == currentLvlName).Checkpoints[e.Node.Index].SlamEnabled;
            cpPowerBlockEnabled.Checked = map.Levels.First(x => x.Name == currentLvlName).Checkpoints[e.Node.Index].PowerblockEnabled;
            isEffLocked.Checked = map.Levels.First(x => x.Name == currentLvlName).Checkpoints[e.Node.Index].EffectLock;
            punchUpDown.Value = decimal.Parse(map.Levels.First(x => x.Name == currentLvlName).Checkpoints[e.Node.Index].PunchCount);
            slamUpDown.Value = decimal.Parse(map.Levels.First(x => x.Name == currentLvlName).Checkpoints[e.Node.Index].SlamCount);
            powerBlockUpDown.Value = decimal.Parse(map.Levels.First(x => x.Name == currentLvlName).Checkpoints[e.Node.Index].PowerblockCount);
            isEffLocked.Enabled = map.Levels.First(x => x.Name == currentLvlName).Checkpoints[e.Node.Index].Effects.Any(x => x.Type == EffectType.Ability || x.Type == EffectType.Time);
            if (map.Levels.First(x => x.Name == currentLvlName).Checkpoints[e.Node.Index].Type == CheckpointType.LevelStart || map.Levels.First(x => x.Name == currentLvlName).Checkpoints[e.Node.Index].Type == CheckpointType.LevelEnd)
            {
                isNormalCP.Enabled = false;
            }
            isTeleport.Enabled = true;
            isTeleport.Checked = map.Levels.First(x => x.Name == currentLvlName).Checkpoints[e.Node.Index].TeleportCoordinate != "";
            tpCoordTxt.Text = map.Levels.First(x => x.Name == currentLvlName).Checkpoints[e.Node.Index].TeleportCoordinate;
            tpRadTxt.Value = decimal.Parse(map.Levels.First(x => x.Name == currentLvlName).Checkpoints[e.Node.Index].TeleportRadius);
            return;
        }

        if (e.Node.Text.Contains("Effect:")) //effect of a checkpoint selected
        {
            generalGroupBox.Visible = false;
            int effindex = e.Node.Index;
            int cpindex = 1, lvlindex = 1;
            bool mapcp = e.Node.Parent.Parent.Text == "Checkpoint 0";
            mapTreeView.SelectedNode.Tag = e.Node.Index;

            if (mapcp == false)
            {
                cpindex = e.Node.Parent.Parent.Index;
                lvlindex = e.Node.Parent.Parent.Parent.Index - 1;
            }

            Effect currenteff = (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects)[effindex];
            addEffBtn.Text = "Edit Effect";
            cpSettingsGroupBox.Visible = false;
            lvlGroupBox.Visible = false;
            missSettingsGroupBox.Visible = false;
            effSettingsGroupBox.Visible = true;

            if (currenteff.Type == EffectType.Time)
            {
                effTypeComboBox.SelectedIndex = 0;
                effTypeComboBox.Enabled = true;
                effTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                effCoordTxt.Text = currenteff.Coordinate;
                effRadTxt.Value = decimal.Parse(currenteff.Radius.Trim('-'));
                effTimeUpDown.Enabled = true;
                iseffLightShaft.Checked = currenteff.Lightshaft;
            }

            if (currenteff.Type == EffectType.Death)
            {
                effTypeComboBox.SelectedIndex = 1;
                effTypeComboBox.Enabled = true;
                effCoordTxt.Text = currenteff.Coordinate;
                effRadTxt.Value = decimal.Parse(currenteff.Radius.Trim('-'));
                iseffLightShaft.Checked = currenteff.Lightshaft;
            }

            if (currenteff.Type == EffectType.Ability)
            {
                effTypeComboBox.SelectedIndex = 2;
                effTypeComboBox.Enabled = true;
                effCoordTxt.Text = currenteff.Coordinate;
                effRadTxt.Value = decimal.Parse(currenteff.Radius.Trim('-'));
                iseffLightShaft.Checked = currenteff.Lightshaft;
                issEffCdReset.Checked = currenteff.CDReset;
                isNoChange.Checked = currenteff.NoChange;
                effSlamEnabled.Checked = currenteff.SlamEnabled;
                effPunchEnabled.Checked = currenteff.PunchEnabled;
                effPowerBlockEnabled.Checked = currenteff.PowerblockEnabled;
            }

            if (currenteff.Type == EffectType.Permeation)
            {
                effTypeComboBox.SelectedIndex = 3;
                effTypeComboBox.Enabled = true;
                effCoordTxt.Text = currenteff.Coordinate;
                effRadTxt.Value = decimal.Parse(currenteff.Radius.Trim('-'));
                iseffLightShaft.Checked = currenteff.Lightshaft;
                issEffCdReset.Checked = currenteff.CDReset;
                isNoChange.Checked = currenteff.NoChange;
                effSlamEnabled.Checked = currenteff.SlamEnabled;
                effPunchEnabled.Checked = currenteff.PunchEnabled;
                effPowerBlockEnabled.Checked = currenteff.PowerblockEnabled;
            }

            if (currenteff.Type == EffectType.Safepoint)
            {
                effTypeComboBox.SelectedIndex = 4;
                effTypeComboBox.Enabled = true;
                effCoordTxt.Text = currenteff.Coordinate;
                effRadTxt.Value = decimal.Parse(currenteff.Radius.Trim('-'));
                iseffLightShaft.Checked = currenteff.Lightshaft;
                issEffCdReset.Checked = false;
                isNoChange.Checked = false;
                effSlamEnabled.Checked = currenteff.SlamEnabled;
                effPunchEnabled.Checked = currenteff.PunchEnabled;
                effPowerBlockEnabled.Checked = currenteff.PowerblockEnabled;
            }

            if (currenteff.Type == EffectType.EntryPortal)
            {
                effTypeComboBox.SelectedIndex = 5;
                effTypeComboBox.Enabled = false;
                effCoordTxt.Text = currenteff.Coordinate;
                effRadTxt.Value = decimal.Parse(currenteff.Radius);
                iseffLightShaft.Checked = false;
                issEffCdReset.Checked = currenteff.CDReset;
                isNoChange.Checked = currenteff.NoChange;
                effSlamEnabled.Checked = currenteff.SlamEnabled;
                effPunchEnabled.Checked = currenteff.PunchEnabled;
                effPowerBlockEnabled.Checked = currenteff.PowerblockEnabled;
            }

            if (currenteff.Type == EffectType.ExitPortal)
            {
                effTypeComboBox.SelectedIndex = 6;
                effTypeComboBox.Enabled = false;
                effCoordTxt.Text = currenteff.Coordinate;
                effRadTxt.Value = decimal.Parse(currenteff.Radius);
                iseffLightShaft.Checked = false;
                issEffCdReset.Checked = currenteff.CDReset;
                isNoChange.Checked = currenteff.NoChange;
                effSlamEnabled.Checked = currenteff.SlamEnabled;
                effPunchEnabled.Checked = currenteff.PunchEnabled;
                effPowerBlockEnabled.Checked = currenteff.PowerblockEnabled;
            }
            return;
        }

        if (e.Node.Text.Contains("Mission:")) //mission of a checkpoint selected
        {
            generalGroupBox.Visible = false;
            e.Node.Tag = e.Node.Index.ToString();
            int missindex = e.Node.Index;
            int cpindex = 1, lvlindex = 1;
            bool mapcp = e.Node.Parent.Parent.Text == "Checkpoint 0";


            if (mapcp == false)
            {
                cpindex = e.Node.Parent.Parent.Index;
                lvlindex = e.Node.Parent.Parent.Parent.Index - 1;
            }

            Mission currenteff = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions)[missindex];
            switch (currenteff.Type)
            {
                case MissionType.NoRocketPunch:
                    missTypeComboBox.SelectedIndex = 0;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                    break;
                case MissionType.NoSlam:
                    missTypeComboBox.SelectedIndex = 2;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                    break;
                case MissionType.NoPowerblock:
                    missTypeComboBox.SelectedIndex = 1;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                    break;
                case MissionType.Stalless:
                    missTypeComboBox.SelectedIndex = 3;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                    break;
                case MissionType.Spin:
                    missTypeComboBox.SelectedIndex = 4;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                    break;
                case MissionType.RocketPunchFirst:
                    missTypeComboBox.SelectedIndex = 5;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                    break;
                case MissionType.PowerblockFirst:
                    missTypeComboBox.SelectedIndex = 6;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                    break;
                case MissionType.SlamFirst:
                    missTypeComboBox.SelectedIndex = 7;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                    break;
                case MissionType.UpwardsDiag:
                    missTypeComboBox.SelectedIndex = 8;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                    break;
                case MissionType.DownwardsDiag:
                    missTypeComboBox.SelectedIndex = 9;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                    break;
                case MissionType.PunchBounce:
                    missTypeComboBox.SelectedIndex = 10;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                    break;
            }

            addMissBtn.Text = "Edit Mission";
            cpSettingsGroupBox.Visible = false;
            lvlGroupBox.Visible = false;
            missSettingsGroupBox.Visible = true;
            effSettingsGroupBox.Visible = false;
        }
    }

    public void ClearCPControls()
    {
        cpCoordTxt.Text = "";
        tpCoordTxt.Text = "";
        cpRadTxt.Value = 2;
        tpRadTxt.Value = 2;
    }

    private void addLvlBtn_Click(object sender, EventArgs e)
    {
        if (addLvlBtn.Text == "Add Level")
        {
            if (string.IsNullOrWhiteSpace(lvlNameTxt.Text) || string.IsNullOrEmpty(lvlNameTxt.Text)) //lvlNameTxt was empty
            {
                MessageBox.Show("Please enter a level name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (map.Levels.Any(x => x.Name == lvlNameTxt.Text)) //user put a level name that already exists
            {
                MessageBox.Show("A level with this name already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            Level newLvl = new Level();
            newLvl.Name = lvlNameTxt.Text;
            newLvl.Icon = lvlIconComboBox.Items[lvlIconComboBox.SelectedIndex].ToString();
            newLvl.Color = $"Color({lvlColorComboBox.Items[lvlColorComboBox.SelectedIndex].ToString()})";
            map.Levels.Add(newLvl);
            TreeNode lvlNode = new TreeNode("Level - " + lvlNameTxt.Text);
            lvlNode.Tag = lvlNameTxt.Text;
            mapTreeView.Nodes[0].Nodes.Add(lvlNode);
            mapTreeView.SelectedNode = mapTreeView.Nodes[0].LastNode;
            return;
        }

        if (addLvlBtn.Text == "Edit Level")
        {
            if (string.IsNullOrWhiteSpace(lvlNameTxt.Text) || string.IsNullOrEmpty(lvlNameTxt.Text)) //lvlnameTxt was empty
            {
                MessageBox.Show("Please enter a level name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (mapTreeView.SelectedNode.Tag.ToString() == lvlNameTxt.Text || !map.Levels.Any(x => x.Name == lvlNameTxt.Text)) //incredible stuff
            {
                string oldname = mapTreeView.SelectedNode.Tag.ToString();
                map.Levels.First(x => x.Name == oldname).Icon = lvlIconComboBox.Items[lvlIconComboBox.SelectedIndex].ToString();
                map.Levels.First(x => x.Name == oldname).Color = $"Color({lvlColorComboBox.Items[lvlColorComboBox.SelectedIndex].ToString()})";
                map.Levels.First(x => x.Name == oldname).Name = lvlNameTxt.Text;
                mapTreeView.SelectedNode.Tag = lvlNameTxt.Text;
                mapTreeView.SelectedNode.Text = "Level - " + lvlNameTxt.Text;
                int index = mapTreeView.SelectedNode.Index;
                mapTreeView.SelectedNode = mapTreeView.Nodes[0];
                mapTreeView.SelectedNode = mapTreeView.Nodes[0].Nodes[index];
                return;
            }

            if (map.Levels.Any(x => x.Name == lvlNameTxt.Text)) //user put a level name that already exists
            {
                MessageBox.Show("A level with this name already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public bool IsAbilCount()
    {
        //if any of the NumericUpDown in the abilbox has Value > 0
        if (abilCountGroupBox.Controls.OfType<NumericUpDown>().Any(n => n.Value > 0))
        {
            return true;
        }

        return false;
    }

    private void addCpBtn_Click(object sender, EventArgs e)
    {
        if (addCpBtn.Text == "Add Checkpoint")
        {
            var result = Regex.Match(cpCoordTxt.Text, pattern);
            var tpresult = Regex.Match(tpCoordTxt.Text, pattern);
            if (!result.Success)
            {
                MessageBox.Show("There is something wrong with the checkpoint coordinates.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (isTeleport.Checked && !tpresult.Success)
            {
                MessageBox.Show("There is something wrong with the teleport checkpoint coordinates.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (isLvlSelector.Checked)
            {
                if (map.levelSelectorCP.Coordinate != "")
                {
                    MessageBox.Show("There is already a level selector checkpoint!" +
                        Environment.NewLine + "You can edit the existing level selector checkpoint.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Checkpoint newCP = new Checkpoint();
                newCP.Coordinate = cpCoordTxt.Text;
                newCP.Radius = cpRadTxt.Value.ToString();
                newCP.Prime = isEffLocked.Checked ? (GeneratePrimeForCP() * 11 * 17).ToString() : (GeneratePrimeForCP() * 11).ToString();
                newCP.Type = CheckpointType.LevelSelector;
                newCP.EffectLock = isEffLocked.Checked;
                newCP.PunchEnabled = cpPunchEnabled.Checked;
                newCP.SlamEnabled = cpSlamEnabled.Checked;
                newCP.PowerblockEnabled = cpPowerBlockEnabled.Checked;
                newCP.isAbilCount = IsAbilCount();
                newCP.PunchCount = punchUpDown.Value.ToString();
                newCP.SlamCount = slamUpDown.Value.ToString();
                newCP.PowerblockCount = powerBlockUpDown.Value.ToString();
                mapTreeView.TopNode.Nodes.Add("Checkpoint 0");
                mapTreeView.TopNode.Nodes[0].Nodes.Add("Effects");
                mapTreeView.TopNode.Nodes[0].Nodes.Add("Missions");
                map.levelSelectorCP = newCP;
                mapTreeView.SelectedNode = mapTreeView.TopNode.FirstNode;
                mapTreeView.SelectedNode = mapTreeView.TopNode;
                return;
            }

            if (isLvlStartCP.Checked)
            {
                string currentLvlName = mapTreeView.SelectedNode.Tag.ToString();
                if (map.Levels.First(x => x.Name == currentLvlName).Checkpoints.Any(x => x.Type == CheckpointType.LevelStart))
                {
                    MessageBox.Show("There is already a level start checkpoint for this level!" +
                        Environment.NewLine + "You can edit the existing level start checkpoint.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Checkpoint newCP = new Checkpoint();
                newCP.Coordinate = cpCoordTxt.Text;
                newCP.Prime = isEffLocked.Checked ? (GeneratePrimeForCP() * 13 * 17).ToString() : (GeneratePrimeForCP() * 13).ToString();
                newCP.Radius = cpRadTxt.Value.ToString();
                newCP.Type = CheckpointType.LevelStart;
                newCP.TeleportCoordinate = tpCoordTxt.Text;
                newCP.TeleportRadius = tpRadTxt.Value.ToString();
                newCP.EffectLock = isEffLocked.Checked;
                newCP.PunchEnabled = cpPunchEnabled.Checked;
                newCP.SlamEnabled = cpSlamEnabled.Checked;
                newCP.PowerblockEnabled = cpPowerBlockEnabled.Checked;
                newCP.isAbilCount = IsAbilCount();
                newCP.PunchCount = punchUpDown.Value.ToString();
                newCP.SlamCount = slamUpDown.Value.ToString();
                newCP.PowerblockCount = powerBlockUpDown.Value.ToString();
                map.Levels.First(x => x.Name == mapTreeView.SelectedNode.Tag.ToString()).Checkpoints.Insert(0, newCP);
                bool doesLvlFirstCpExist = map.Levels.First(x => x.Name == currentLvlName).Checkpoints.Any(x => x.Type == CheckpointType.LevelStart);
                TreeNode cpNode = new TreeNode("Checkpoint 1");
                cpNode.Nodes.Add("Effects");
                cpNode.Nodes.Add("Missions");
                if (!doesLvlFirstCpExist)
                {
                    for (int i = 0; i < mapTreeView.SelectedNode.Nodes.Count; i++)
                    {
                        mapTreeView.SelectedNode.Nodes[i].Text = "Checkpoint " + (i + 1).ToString();
                    }
                    mapTreeView.SelectedNode = mapTreeView.SelectedNode.Nodes[0].Nodes[0];
                    return;
                }
                mapTreeView.SelectedNode.Nodes.Add(cpNode);
                mapTreeView.SelectedNode = mapTreeView.SelectedNode.FirstNode;
                mapTreeView.SelectedNode = mapTreeView.SelectedNode.Parent;
                saveBtn.Enabled = true;
                clipboardLbl.Enabled = true;
                return;
            }

            if (isLvlEndCP.Checked)
            {
                string currentLvlName = mapTreeView.SelectedNode.Tag.ToString();
                if (map.Levels.First(x => x.Name == currentLvlName).Checkpoints.Any(x => x.Type == CheckpointType.LevelEnd))
                {
                    MessageBox.Show("There is already a level end checkpoint for this level!" +
                        Environment.NewLine + "You can edit the existing level end checkpoint.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Checkpoint newCP = new Checkpoint();
                newCP.Coordinate = cpCoordTxt.Text;
                newCP.Prime = "True";
                newCP.Radius = cpRadTxt.Value.ToString();
                newCP.Type = CheckpointType.LevelEnd;
                newCP.isAbilCount = false;
                map.Levels.First(x => x.Name == mapTreeView.SelectedNode.Tag.ToString()).Checkpoints.Add(newCP);
                TreeNode cpNode = new TreeNode($"Checkpoint {map.Levels.First(x => x.Name == mapTreeView.SelectedNode.Tag.ToString()).Checkpoints.Count}");
                mapTreeView.SelectedNode.Nodes.Add(cpNode);
                mapTreeView.SelectedNode = mapTreeView.TopNode;
                saveBtn.Enabled = true;
                clipboardLbl.Enabled = true;
                return;
            }

            if (isNormalCP.Checked)
            {
                string currentLvlName = mapTreeView.SelectedNode.Tag.ToString();
                int levelendIndex = map.Levels.First(x => x.Name == currentLvlName).Checkpoints.FindIndex(x => x.Type == CheckpointType.LevelEnd);
                int wheretoinsert = levelendIndex == -1 ? map.Levels.First(x => x.Name == currentLvlName).Checkpoints.Count : levelendIndex;
                bool doesLvlLastCpExist = map.Levels.First(x => x.Name == currentLvlName).Checkpoints.Any(x => x.Type == CheckpointType.LevelEnd);
                Checkpoint newCP = new Checkpoint();
                newCP.Coordinate = cpCoordTxt.Text;
                newCP.Prime = isEffLocked.Checked ? (GeneratePrimeForCP() * 17).ToString() : GeneratePrimeForCP().ToString();
                newCP.Radius = cpRadTxt.Value.ToString();
                newCP.Type = CheckpointType.Normal;
                newCP.TeleportCoordinate = tpCoordTxt.Text;
                newCP.TeleportRadius = tpRadTxt.Value.ToString();
                newCP.EffectLock = isEffLocked.Checked;
                newCP.PunchEnabled = cpPunchEnabled.Checked;
                newCP.SlamEnabled = cpSlamEnabled.Checked;
                newCP.PowerblockEnabled = cpPowerBlockEnabled.Checked;
                newCP.isAbilCount = IsAbilCount();
                newCP.PunchCount = punchUpDown.Value.ToString();
                newCP.SlamCount = slamUpDown.Value.ToString();
                newCP.PowerblockCount = powerBlockUpDown.Value.ToString();

                if (doesLvlLastCpExist)
                {
                    //place this checkpoint before the level end checkpoint
                    map.Levels.First(x => x.Name == mapTreeView.SelectedNode.Tag.ToString()).Checkpoints.Insert(wheretoinsert, newCP);
                    TreeNode _cpNode = new TreeNode($"Checkpoint {map.Levels.First(x => x.Name == mapTreeView.SelectedNode.Tag.ToString()).Checkpoints.Count}");
                    _cpNode.Nodes.Add("Effects");
                    _cpNode.Nodes.Add("Missions");
                    mapTreeView.SelectedNode.Nodes.Insert(wheretoinsert, _cpNode);
                    //change all the cp nodes to the correct number
                    for (int i = 0; i < mapTreeView.SelectedNode.Nodes.Count; i++)
                    {
                        mapTreeView.SelectedNode.Nodes[i].Text = "Checkpoint " + (i + 1).ToString();
                    }
                    saveBtn.Enabled = true;
                    clipboardLbl.Enabled = true;
                    return;
                }
                //if last cp doesn't exist add it to the end
                map.Levels.First(x => x.Name == mapTreeView.SelectedNode.Tag.ToString()).Checkpoints.Add(newCP);
                TreeNode cpNode = new TreeNode($"Checkpoint {map.Levels.First(x => x.Name == mapTreeView.SelectedNode.Tag.ToString()).Checkpoints.Count}");
                cpNode.Nodes.Add("Effects");
                cpNode.Nodes.Add("Missions");
                mapTreeView.SelectedNode.Nodes.Add(cpNode);
                mapTreeView.SelectedNode = mapTreeView.SelectedNode.FirstNode;
                mapTreeView.SelectedNode = mapTreeView.SelectedNode.Parent;
                saveBtn.Enabled = true;
                clipboardLbl.Enabled = true;
                return;
            }
        }

        if (addCpBtn.Text == "Edit Checkpoint")
        {
            var result = Regex.Match(cpCoordTxt.Text, pattern);
            var tpresult = Regex.Match(tpCoordTxt.Text, pattern);

            int cpindex = mapTreeView.SelectedNode.Index;
            needed.Text = mapTreeView.SelectedNode.Parent.Index.ToString();
            int killme = int.Parse(needed.Text);
            if (!result.Success)
            {
                MessageBox.Show("There is something wrong with the checkpoint coordinates.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (isTeleport.Checked && !tpresult.Success)
            {
                MessageBox.Show("There is something wrong with the teleport checkpoint coordinates.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (isLvlSelector.Checked)
            {
                map.levelSelectorCP.Coordinate = cpCoordTxt.Text;
                map.levelSelectorCP.Prime = isEffLocked.Checked ? (GeneratePrimeForCP() * 11 * 17).ToString() : (GeneratePrimeForCP() * 11).ToString();
                map.levelSelectorCP.Radius = cpRadTxt.Value.ToString();
                map.levelSelectorCP.PunchEnabled = cpPunchEnabled.Checked;
                map.levelSelectorCP.SlamEnabled = cpSlamEnabled.Checked;
                map.levelSelectorCP.PowerblockEnabled = cpPowerBlockEnabled.Checked;
                map.levelSelectorCP.isAbilCount = IsAbilCount();
                map.levelSelectorCP.PunchCount = punchUpDown.Value.ToString();
                map.levelSelectorCP.SlamCount = slamUpDown.Value.ToString();
                map.levelSelectorCP.PowerblockCount = powerBlockUpDown.Value.ToString();
                map.levelSelectorCP.EffectLock = isEffLocked.Checked;
                mapTreeView.SelectedNode = mapTreeView.TopNode;
                return;
            }

            if (isLvlStartCP.Checked)
            {
                string currentLvlName = mapTreeView.SelectedNode.Parent.Tag.ToString();
                if (map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].Type != CheckpointType.LevelStart && map.Levels.First(x => x.Name == currentLvlName).Checkpoints.Any(x => x.Type == CheckpointType.LevelStart))
                {
                    MessageBox.Show("There is already a level start checkpoint for this level!" +
                        Environment.NewLine + "You can edit/delete the existing level start checkpoint.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].Coordinate = cpCoordTxt.Text;
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].Radius = cpRadTxt.Value.ToString();
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].TeleportCoordinate = tpCoordTxt.Text;
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].TeleportRadius = tpRadTxt.Value.ToString();
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].GoBackCP = 0;
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].Prime = isEffLocked.Checked ? (GeneratePrimeForCP() * 13 * 17).ToString() : (GeneratePrimeForCP() * 13).ToString();
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].PunchEnabled = cpPunchEnabled.Checked;
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].SlamEnabled = cpSlamEnabled.Checked;
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].PowerblockEnabled = cpPowerBlockEnabled.Checked;
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].isAbilCount = IsAbilCount();
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].PunchCount = punchUpDown.Value.ToString();
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].SlamCount = slamUpDown.Value.ToString();
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].PowerblockCount = powerBlockUpDown.Value.ToString();
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].Type = CheckpointType.LevelStart;
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].EffectLock = isEffLocked.Checked;
                Checkpoint newThis = map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex];
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints.RemoveAt(cpindex);
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints.Insert(0, newThis);
                TreeNode cpNode = new TreeNode($"Checkpoint 1");
                cpNode.Nodes.Add("Effects");
                cpNode.Nodes.Add("Missions");
                if (newThis.Effects.Any())
                {
                    foreach (var effect in newThis.Effects)
                    {
                        cpNode.Nodes[0].Nodes.Add(effect.ToNodeString());
                    }
                }

                if (newThis.Missions.Any())
                {
                    foreach (var mission in newThis.Missions)
                    {
                        cpNode.Nodes[1].Nodes.Add(mission.ToNodeString());
                    }
                }
                mapTreeView.SelectedNode.Parent.Nodes.Insert(0, cpNode);
                currentLvlName = map.Levels.First(x => x.Name == currentLvlName).Name;
                var lvnode = mapTreeView.SelectedNode.Parent;
                mapTreeView.SelectedNode.Remove();
                mapTreeView.SelectedNode = lvnode;
                //change the name of the other checkpoints accordingly
                for (int i = 0; i < mapTreeView.SelectedNode.Nodes.Count; i++)
                {
                    mapTreeView.SelectedNode.Nodes[i].Text = $"Checkpoint {i + 1}";
                }
                saveBtn.Enabled = true;
                clipboardLbl.Enabled = true;
                return;
            }

            if (isLvlEndCP.Checked)
            {
                string currentLvlName = mapTreeView.SelectedNode.Parent.Tag.ToString();
                if (map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].Type != CheckpointType.LevelEnd && map.Levels.First(x => x.Name == currentLvlName).Checkpoints.Any(x => x.Type == CheckpointType.LevelEnd))
                {
                    MessageBox.Show("There is already a level end checkpoint for this level!" +
                        Environment.NewLine + "You can edit/delete the existing level end checkpoint.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].Coordinate = cpCoordTxt.Text;
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].Radius = cpRadTxt.Value.ToString();
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].TeleportCoordinate = "";
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].TeleportRadius = tpRadTxt.Value.ToString();
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].Prime = "True";
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].PunchEnabled = false;
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].SlamEnabled = false;
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].PowerblockEnabled = false;
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].isAbilCount = false;
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].PunchCount = "0";
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].SlamCount = "0";
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].PowerblockCount = "0";
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].Type = CheckpointType.LevelEnd;
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].EffectLock = false;
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].Effects.Clear();
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].Missions.Clear();
                Checkpoint newThis = map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex];
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints.RemoveAt(cpindex);
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints.Add(newThis);
                TreeNode cpNode = new TreeNode($"Checkpoint {map.Levels.First(x => x.Name == currentLvlName).Checkpoints.Count}");
                mapTreeView.SelectedNode.Parent.Nodes.Add(cpNode);
                currentLvlName = map.Levels.First(x => x.Name == currentLvlName).Name;
                var lvnode = mapTreeView.SelectedNode.Parent;
                mapTreeView.SelectedNode.Remove();
                mapTreeView.SelectedNode = lvnode;
                for (int i = 0; i < mapTreeView.SelectedNode.Nodes.Count; i++)
                {
                    mapTreeView.SelectedNode.Nodes[i].Text = $"Checkpoint {i + 1}";
                }
                saveBtn.Enabled = true;
                clipboardLbl.Enabled = true;
                return;
            }

            if (isNormalCP.Checked)
            {
                string currentLvlName = mapTreeView.SelectedNode.Parent.Tag.ToString();
                if (map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].Type == CheckpointType.LevelEnd)
                {
                    mapTreeView.SelectedNode.Nodes.Add("Effects");
                    mapTreeView.SelectedNode.Nodes.Add("Missions");
                }

                bool doesLvlLastCpExist = map.Levels.First(x => x.Name == currentLvlName).Checkpoints.Any(x => x.Type == CheckpointType.LevelEnd);
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].Coordinate = cpCoordTxt.Text;
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].Radius = cpRadTxt.Value.ToString();
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].TeleportCoordinate = tpCoordTxt.Text;
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].TeleportRadius = tpRadTxt.Value.ToString();
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].Prime = isEffLocked.Checked ? (GeneratePrimeForCP() * 17).ToString() : GeneratePrimeForCP().ToString();
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].PunchEnabled = cpPunchEnabled.Checked;
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].SlamEnabled = cpSlamEnabled.Checked;
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].PowerblockEnabled = cpPowerBlockEnabled.Checked;
                if (IsAbilCount())
                {
                    map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].isAbilCount = true;
                    map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].PunchCount = punchUpDown.Value.ToString();
                    map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].SlamCount = slamUpDown.Value.ToString();
                    map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].PowerblockCount = powerBlockUpDown.Value.ToString();
                }
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].Type = CheckpointType.Normal;
                map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].EffectLock = isEffLocked.Checked;
                mapTreeView.SelectedNode = mapTreeView.SelectedNode.Parent;
                for (int i = 0; i < mapTreeView.SelectedNode.Nodes.Count; i++)
                {
                    mapTreeView.SelectedNode.Nodes[i].Text = $"Checkpoint {i + 1}";
                }
                saveBtn.Enabled = true;
                clipboardLbl.Enabled = true;
            }
        }
    }

    private void cpCoordTxt_KeyPress(object sender, KeyPressEventArgs e)
    {
        int dotcount = cpCoordTxt.Text.Count(f => f == '.');
        int commacount = cpCoordTxt.Text.Count(f => f == ',');
        int eksicount = cpCoordTxt.Text.Count(f => f == '-');

        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != '-' && e.KeyChar != ',')
        {
            e.Handled = true;
        }

        if (e.KeyChar == '.' && cpCoordTxt.Text.EndsWith("."))
        {
            e.Handled = true;
        }

        if (e.KeyChar == ',' && cpCoordTxt.Text.EndsWith(","))
        {
            e.Handled = true;
        }

        if (e.KeyChar == '-' && cpCoordTxt.Text.EndsWith("-"))
        {
            e.Handled = true;
        }

        if (e.KeyChar == '-' && cpCoordTxt.Text.Length > 0 && cpCoordTxt.Text.EndsWith(",") && eksicount != 3)
        {
            e.Handled = false;
            return;
        }

        if (e.KeyChar == ',' && cpCoordTxt.Text.EndsWith(".") || e.KeyChar == ',' && cpCoordTxt.Text.EndsWith("-") || e.KeyChar == ',' && cpCoordTxt.Text == string.Empty)
        {
            e.Handled = true;
        }

        if (e.KeyChar == '-' && cpCoordTxt.Text.EndsWith(",") || e.KeyChar == '-' && cpCoordTxt.Text.EndsWith("."))
        {
            e.Handled = true;
        }

        if (e.KeyChar == '.' && cpCoordTxt.Text.EndsWith(",") || e.KeyChar == '.' && cpCoordTxt.Text.EndsWith("-") || e.KeyChar == '.' && cpCoordTxt.Text == string.Empty)
        {
            e.Handled = true;
        }

        if (e.KeyChar == '.' && dotcount == 3) e.Handled = true;
        if (e.KeyChar == ',' && commacount == 2) e.Handled = true;
        if (e.KeyChar == '-' && eksicount == 3) e.Handled = true;
    }

    private void tpCoordTxt_KeyPress(object sender, KeyPressEventArgs e)
    {
        int dotcount = tpCoordTxt.Text.Count(f => f == '.');
        int commacount = tpCoordTxt.Text.Count(f => f == ',');
        int eksicount = tpCoordTxt.Text.Count(f => f == '-');

        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != '-' && e.KeyChar != ',')
        {
            e.Handled = true;
        }

        if (e.KeyChar == '.' && tpCoordTxt.Text.EndsWith("."))
        {
            e.Handled = true;
        }

        if (e.KeyChar == ',' && tpCoordTxt.Text.EndsWith(","))
        {
            e.Handled = true;
        }

        if (e.KeyChar == '-' && tpCoordTxt.Text.EndsWith("-"))
        {
            e.Handled = true;
        }

        if (char.IsWhiteSpace(e.KeyChar) && tpCoordTxt.Text.EndsWith(" "))
        {
            e.Handled = true;
        }

        if (e.KeyChar == '-' && tpCoordTxt.Text.Length > 0 && tpCoordTxt.Text.EndsWith(",") && eksicount != 3)
        {
            e.Handled = false;
            return;
        }

        if (e.KeyChar == ',' && tpCoordTxt.Text.EndsWith(".") || e.KeyChar == ',' && tpCoordTxt.Text.EndsWith("-") || e.KeyChar == ',' && tpCoordTxt.Text == string.Empty)
        {
            e.Handled = true;
        }

        if (e.KeyChar == '-' && tpCoordTxt.Text.EndsWith(",") || e.KeyChar == '-' && tpCoordTxt.Text.EndsWith("."))
        {
            e.Handled = true;
        }

        if (e.KeyChar == '.' && tpCoordTxt.Text.EndsWith(",") || e.KeyChar == '.' && tpCoordTxt.Text.EndsWith("-") || e.KeyChar == ',' && tpCoordTxt.Text == string.Empty)
        {
            e.Handled = true;
        }

        if (e.KeyChar == '.' && dotcount == 3) e.Handled = true;
        if (e.KeyChar == ',' && commacount == 2) e.Handled = true;
        if (e.KeyChar == '-' && eksicount == 3) e.Handled = true;
    }

    private void effCoordTxt_KeyPress(object sender, KeyPressEventArgs e)
    {
        int dotcount = effCoordTxt.Text.Count(f => f == '.');
        int commacount = effCoordTxt.Text.Count(f => f == ',');
        int eksicount = effCoordTxt.Text.Count(f => f == '-');

        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != '-' && e.KeyChar != ',')
        {
            e.Handled = true;
        }

        if (e.KeyChar == '.' && effCoordTxt.Text.EndsWith("."))
        {
            e.Handled = true;
        }

        if (e.KeyChar == ',' && effCoordTxt.Text.EndsWith(","))
        {
            e.Handled = true;
        }

        if (e.KeyChar == '-' && effCoordTxt.Text.EndsWith("-"))
        {
            e.Handled = true;
        }

        if (e.KeyChar == '-' && effCoordTxt.Text.Length > 0 && effCoordTxt.Text.EndsWith(",") && eksicount != 3)
        {
            e.Handled = false;
            return;
        }

        if (e.KeyChar == ',' && effCoordTxt.Text.EndsWith(".") || e.KeyChar == ',' && effCoordTxt.Text.EndsWith("-") || e.KeyChar == ',' && effCoordTxt.Text == string.Empty)
        {
            e.Handled = true;
        }

        if (e.KeyChar == '-' && effCoordTxt.Text.EndsWith(",") || e.KeyChar == '-' && effCoordTxt.Text.EndsWith("."))
        {
            e.Handled = true;
        }

        if (e.KeyChar == '.' && effCoordTxt.Text.EndsWith(",") || e.KeyChar == '.' && effCoordTxt.Text.EndsWith("-") || e.KeyChar == '.' && effCoordTxt.Text == string.Empty)
        {
            e.Handled = true;
        }

        if (e.KeyChar == '.' && dotcount == 3) e.Handled = true;
        if (e.KeyChar == ',' && commacount == 2) e.Handled = true;
        if (e.KeyChar == '-' && eksicount == 3) e.Handled = true;
    }

    private void cpType_CheckedChanged(object sender, EventArgs e)
    {
        RadioButton cb = (RadioButton)sender;
        if (cb.Text == "Level End Checkpoint" && cb.Checked == true)
        {
            punchUpDown.Value = 0;
            slamUpDown.Value = 0;
            powerBlockUpDown.Value = 0;
            cpAbilGroupBox.Enabled = false;
            abilCountGroupBox.Enabled = false;
            isEffLocked.Checked = false;
            isEffLocked.Enabled = false;
            isTeleport.Checked = false;
            isTeleport.Enabled = false;
            tpCoordTxt.Enabled = false;
            tpRadTxt.Enabled = false;
            return;
        }

        if (cb.Text == "Level Selector Checkpoint")
        {
            punchUpDown.Value = 0;
            slamUpDown.Value = 0;
            powerBlockUpDown.Value = 0;
            cpAbilGroupBox.Enabled = true;
            abilCountGroupBox.Enabled = true;
            isEffLocked.Checked = false;
            isEffLocked.Enabled = false;
            isTeleport.Checked = false;
            isTeleport.Enabled = false;
            tpCoordTxt.Enabled = false;
            tpRadTxt.Enabled = false;
            return;
        }
        isEffLocked.Enabled = true;
        punchUpDown.Value = 0;
        slamUpDown.Value = 0;
        powerBlockUpDown.Value = 0;
        cpAbilGroupBox.Enabled = true;
        abilCountGroupBox.Enabled = true;
        isTeleport.Enabled = true;
    }

    private void isTeleport_CheckedChanged_1(object sender, EventArgs e)
    {
        tpCoordTxt.Enabled = isTeleport.Checked;
        tpRadTxt.Enabled = isTeleport.Checked;
    }

    private void addEffBtn_Click(object sender, EventArgs e)
    {
        var result = Regex.Match(effCoordTxt.Text, pattern);

        if (!result.Success)
        {
            MessageBox.Show("There is something wrong with the effect coordinates.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        bool mapcp = false;


        if (addEffBtn.Text == "Add Effect")
        {
            int lvlindex = 1, cpindex = 1;

            if (mapTreeView.SelectedNode.Parent.Text == "Checkpoint 0")
            {
                mapcp = true;
            }

            if (mapTreeView.SelectedNode.Parent.Text != "Checkpoint 0")
            {
                lvlindex = mapTreeView.SelectedNode.Parent.Parent.Index - 1;
                cpindex = mapTreeView.SelectedNode.Parent.Index;
            }

            if (effTypeComboBox.SelectedItem.ToString() == "Time")
            {
                if (effTimeUpDown.Value == 0)
                {
                    MessageBox.Show("Time effect value cannot be 0.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects).Add(new Effect
                {
                    Type = EffectType.Time,
                    Coordinate = effCoordTxt.Text,
                    Radius = iseffLightShaft.Checked ? ("-" + effRadTxt.Value.ToString()) : effRadTxt.Value.ToString(),
                    Lightshaft = iseffLightShaft.Checked,
                    TimeValue = effTimeUpDown.Value.ToString()
                });
                mapTreeView.SelectedNode.Nodes.Add("Effect: Time " + (iseffLightShaft.Checked ? "Lightshaft - " : "Orb - ") + effTimeUpDown.Value.ToString());
                //mapTreeView.SelectedNode = mapTreeView.SelectedNode.Nodes[0].FirstNode;
                mapTreeView.SelectedNode = mapTreeView.SelectedNode.Parent;
                effCoordTxt.Text = "";
                effRadTxt.Value = decimal.Parse("1.1");
                effTimeUpDown.Value = 0;
                iseffLightShaft.Checked = false;
                return;
            }

            if (effTypeComboBox.SelectedItem.ToString() == "Death")
            {
                (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects).Add(new Effect
                {
                    Type = EffectType.Death,
                    Coordinate = effCoordTxt.Text,
                    Radius = iseffLightShaft.Checked ? ("-" + effRadTxt.Value.ToString()) : effRadTxt.Value.ToString(),
                    Lightshaft = iseffLightShaft.Checked
                });

                mapTreeView.SelectedNode.Nodes.Add("Effect: Death " + (iseffLightShaft.Checked ? "Lightshaft" : "Orb"));
                mapTreeView.SelectedNode = mapTreeView.SelectedNode.Parent;
                effCoordTxt.Text = "";
                effRadTxt.Value = decimal.Parse("1.1");
                iseffLightShaft.Checked = false;
                return;
            }

            if (effTypeComboBox.SelectedItem.ToString() == "Ability")
            {
                (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects).Add(new Effect
                {
                    Type = EffectType.Ability,
                    Coordinate = effCoordTxt.Text,
                    Radius = iseffLightShaft.Checked ? ("-" + effRadTxt.Value.ToString()) : effRadTxt.Value.ToString(),
                    Prime = issEffCdReset.Checked ? ("-" + (isNoChange.Checked ? "11" : GeneratePrimeForEffect().ToString())) : (isNoChange.Checked ? "11" : GeneratePrimeForEffect().ToString()),
                    NoChange = isNoChange.Checked,
                    PunchEnabled = effPunchEnabled.Checked,
                    SlamEnabled = effSlamEnabled.Checked,
                    PowerblockEnabled = effPowerBlockEnabled.Checked,
                    Lightshaft = iseffLightShaft.Checked,
                    CDReset = issEffCdReset.Checked
                });
                mapTreeView.SelectedNode.Nodes.Add("Effect: Ability " + (iseffLightShaft.Checked ? "Lightshaft" : "Orb"));
                mapTreeView.SelectedNode = mapTreeView.SelectedNode.Parent;
                effCoordTxt.Text = "";
                effRadTxt.Value = decimal.Parse("1.1");
                iseffLightShaft.Checked = false;
                issEffCdReset.Checked = false;
                isNoChange.Checked = true;
                return;
            }

            if (effTypeComboBox.SelectedItem.ToString() == "Permeation")
            {
                (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects).Add(new Effect
                {
                    Type = EffectType.Permeation,
                    Coordinate = effCoordTxt.Text,
                    Radius = iseffLightShaft.Checked ? ("-" + effRadTxt.Value.ToString()) : effRadTxt.Value.ToString(),
                    Prime = issEffCdReset.Checked ? ("-" + (isNoChange.Checked ? "11" : GeneratePrimeForEffect().ToString())) : (isNoChange.Checked ? "11" : GeneratePrimeForEffect().ToString()),
                    NoChange = isNoChange.Checked,
                    PunchEnabled = effPunchEnabled.Checked,
                    SlamEnabled = effSlamEnabled.Checked,
                    PowerblockEnabled = effPowerBlockEnabled.Checked,
                    Lightshaft = iseffLightShaft.Checked,
                    CDReset = issEffCdReset.Checked
                });
                mapTreeView.SelectedNode.Nodes.Add("Effect: Permeation " + (iseffLightShaft.Checked ? "Lightshaft" : "Orb"));
                mapTreeView.SelectedNode = mapTreeView.SelectedNode.Parent;
                effCoordTxt.Text = "";
                effRadTxt.Value = decimal.Parse("1.1");
                iseffLightShaft.Checked = false;
                issEffCdReset.Checked = false;
                isNoChange.Checked = true;
                return;
            }

            if (effTypeComboBox.SelectedItem.ToString() == "Safepoint")
            {
                (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects).Add(new Effect
                {
                    Type = EffectType.Safepoint,
                    Coordinate = effCoordTxt.Text,
                    Radius = effRadTxt.Value.ToString(),
                    Prime = isNoChange.Checked ? "11" : GeneratePrimeForEffect().ToString(),
                    NoChange = isNoChange.Checked,
                    PunchEnabled = effPunchEnabled.Checked,
                    SlamEnabled = effSlamEnabled.Checked,
                    PowerblockEnabled = effPowerBlockEnabled.Checked,
                });
                mapTreeView.SelectedNode.Nodes.Add("Effect: Safepoint");
                mapTreeView.SelectedNode = mapTreeView.SelectedNode.Parent;
                effCoordTxt.Text = "";
                effRadTxt.Value = decimal.Parse("1.1");
                issEffCdReset.Checked = false;
                isNoChange.Checked = true;
                return;
            }

            if (effTypeComboBox.SelectedItem.ToString() == "Entry Portal")
            {
                (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects).Add(new Effect
                {
                    Type = EffectType.EntryPortal,
                    Coordinate = effCoordTxt.Text,
                    Radius = effRadTxt.Value.ToString(),
                    Prime = issEffCdReset.Checked ? ("-" + (isNoChange.Checked ? "11" : GeneratePrimeForEffect().ToString())) : (isNoChange.Checked ? "11" : GeneratePrimeForEffect().ToString()),
                    NoChange = isNoChange.Checked,
                    PunchEnabled = effPunchEnabled.Checked,
                    SlamEnabled = effSlamEnabled.Checked,
                    PowerblockEnabled = effPowerBlockEnabled.Checked,
                    CDReset = issEffCdReset.Checked
                });
                mapTreeView.SelectedNode.Nodes.Add("Effect: Entry Portal");
                mapTreeView.SelectedNode = mapTreeView.SelectedNode.Parent;
                mapTreeView.SelectedNode = mapTreeView.SelectedNode.Nodes[0];
                mapTreeView.Enabled = false;
                effTypeComboBox.SelectedIndex = 6;
                effTypeComboBox.Enabled = false;
                effCoordTxt.Text = "";
                effRadTxt.Value = decimal.Parse("1.1");
                issEffCdReset.Checked = false;
                isNoChange.Checked = true;
                return;
            }

            if (effTypeComboBox.SelectedItem.ToString() == "Exit Portal")
            {
                (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects).Add(new Effect
                {
                    Type = EffectType.ExitPortal,
                    Coordinate = effCoordTxt.Text,
                    Radius = effRadTxt.Value.ToString(),
                    Prime = issEffCdReset.Checked ? ("-" + (isNoChange.Checked ? "11" : GeneratePrimeForEffect().ToString())) : (isNoChange.Checked ? "11" : GeneratePrimeForEffect().ToString()),
                    NoChange = isNoChange.Checked,
                    PunchEnabled = effPunchEnabled.Checked,
                    SlamEnabled = effSlamEnabled.Checked,
                    PowerblockEnabled = effPowerBlockEnabled.Checked,
                    CDReset = issEffCdReset.Checked
                });
                mapTreeView.SelectedNode.Nodes.Add("Effect: Exit Portal");
                mapTreeView.SelectedNode = mapTreeView.SelectedNode.Parent;
                mapTreeView.Enabled = true;
                effTypeComboBox.SelectedIndex = 0;
                effTypeComboBox.Enabled = true;
                effCoordTxt.Text = "";
                effRadTxt.Value = decimal.Parse("1.1");
                iseffLightShaft.Checked = false;
                issEffCdReset.Checked = false;
                isNoChange.Checked = true;
                return;
            }
        }

        if (addEffBtn.Text == "Edit Effect")
        {
            int lvlindex = 1, cpindex = 1;
            int effindex = int.Parse(mapTreeView.SelectedNode.Tag.ToString());

            if (mapTreeView.SelectedNode.Parent.Parent.Text == "Checkpoint 0")
            {
                mapcp = true;
            }

            if (mapTreeView.SelectedNode.Parent.Parent.Text != "Checkpoint 0")
            {
                lvlindex = mapTreeView.SelectedNode.Parent.Parent.Parent.Index - 1;
                cpindex = mapTreeView.SelectedNode.Parent.Parent.Index;
            }

            Effect currentEff = (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects)[effindex];

            if (effTypeComboBox.SelectedItem.ToString() == "Time")
            {
                if (effTimeUpDown.Value == 0)
                {
                    MessageBox.Show("Time effect value cannot be 0.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (currentEff.Type == EffectType.EntryPortal)
                {
                    (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects).RemoveAt(effindex + 1);
                    mapTreeView.SelectedNode.NextNode.Remove();
                }

                if (currentEff.Type == EffectType.ExitPortal)
                {
                    (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects).RemoveAt(effindex - 1);
                    mapTreeView.SelectedNode.PrevNode.Remove();
                }

                (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects)[effindex] = new Effect
                {
                    Type = EffectType.Time,
                    Coordinate = effCoordTxt.Text,
                    Radius = iseffLightShaft.Checked ? ("-" + effRadTxt.Value.ToString()) : effRadTxt.Value.ToString(),
                    Lightshaft = iseffLightShaft.Checked,
                    TimeValue = effTimeUpDown.Value.ToString()
                };
                mapTreeView.SelectedNode.Text = "Effect: Time " + (iseffLightShaft.Checked ? "Lightshaft - " : "Orb - ") + effTimeUpDown.Value.ToString();
                mapTreeView.SelectedNode = mapTreeView.SelectedNode.Parent;
                return;
            }

            if (effTypeComboBox.SelectedItem.ToString() == "Death")
            {
                if (currentEff.Type == EffectType.EntryPortal)
                {
                    (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects).RemoveAt(effindex + 1);
                    mapTreeView.SelectedNode.NextNode.Remove();
                }

                if (currentEff.Type == EffectType.ExitPortal)
                {
                    (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects).RemoveAt(effindex - 1);
                    mapTreeView.SelectedNode.PrevNode.Remove();
                }

                (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects)[effindex] = new Effect
                {
                    Type = EffectType.Death,
                    Coordinate = effCoordTxt.Text,
                    Radius = iseffLightShaft.Checked ? ("-" + effRadTxt.Value.ToString()) : effRadTxt.Value.ToString(),
                    Lightshaft = iseffLightShaft.Checked
                };

                mapTreeView.SelectedNode.Text = "Effect: Death " + (iseffLightShaft.Checked ? "Lightshaft" : "Orb");
                mapTreeView.SelectedNode = mapTreeView.SelectedNode.Parent;
                return;
            }

            if (effTypeComboBox.SelectedItem.ToString() == "Ability")
            {
                if (currentEff.Type == EffectType.EntryPortal)
                {
                    (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects).RemoveAt(effindex + 1);
                    mapTreeView.SelectedNode.NextNode.Remove();
                }

                if (currentEff.Type == EffectType.ExitPortal)
                {
                    (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects).RemoveAt(effindex - 1);
                    mapTreeView.SelectedNode.PrevNode.Remove();
                }

                (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects)[effindex] = new Effect
                {
                    Type = EffectType.Ability,
                    Coordinate = effCoordTxt.Text,
                    Radius = iseffLightShaft.Checked ? ("-" + effRadTxt.Value.ToString()) : effRadTxt.Value.ToString(),
                    Prime = issEffCdReset.Checked ? ("-" + (isNoChange.Checked ? "11" : GeneratePrimeForEffect().ToString())) : (isNoChange.Checked ? "11" : GeneratePrimeForEffect().ToString()),
                    NoChange = isNoChange.Checked,
                    PunchEnabled = effPunchEnabled.Checked,
                    SlamEnabled = effSlamEnabled.Checked,
                    PowerblockEnabled = effPowerBlockEnabled.Checked,
                    Lightshaft = iseffLightShaft.Checked,
                    CDReset = issEffCdReset.Checked
                };
                mapTreeView.SelectedNode.Text = "Effect: Ability " + (iseffLightShaft.Checked ? "Lightshaft" : "Orb");
                mapTreeView.SelectedNode = mapTreeView.SelectedNode.Parent;
                return;
            }

            if (effTypeComboBox.SelectedItem.ToString() == "Permeation")
            {
                if (currentEff.Type == EffectType.EntryPortal)
                {
                    (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects).RemoveAt(effindex + 1);
                    mapTreeView.SelectedNode.NextNode.Remove();
                }

                if (currentEff.Type == EffectType.ExitPortal)
                {
                    (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects).RemoveAt(effindex - 1);
                    mapTreeView.SelectedNode.PrevNode.Remove();
                }

                (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects)[effindex] = new Effect
                {
                    Type = EffectType.Permeation,
                    Coordinate = effCoordTxt.Text,
                    Radius = iseffLightShaft.Checked ? ("-" + effRadTxt.Value.ToString()) : effRadTxt.Value.ToString(),
                    Prime = issEffCdReset.Checked ? ("-" + (isNoChange.Checked ? "11" : GeneratePrimeForEffect().ToString())) : (isNoChange.Checked ? "11" : GeneratePrimeForEffect().ToString()),
                    NoChange = isNoChange.Checked,
                    PunchEnabled = effPunchEnabled.Checked,
                    SlamEnabled = effSlamEnabled.Checked,
                    PowerblockEnabled = effPowerBlockEnabled.Checked,
                    Lightshaft = iseffLightShaft.Checked,
                    CDReset = issEffCdReset.Checked
                };
                mapTreeView.SelectedNode.Text = "Effect: Permeation " + (iseffLightShaft.Checked ? "Lightshaft" : "Orb");
                mapTreeView.SelectedNode = mapTreeView.SelectedNode.Parent;
                return;
            }

            if (effTypeComboBox.SelectedItem.ToString() == "Safepoint")
            {
                if (currentEff.Type == EffectType.EntryPortal)
                {
                    (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects).RemoveAt(effindex + 1);
                    mapTreeView.SelectedNode.NextNode.Remove();
                }

                if (currentEff.Type == EffectType.ExitPortal)
                {
                    (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects).RemoveAt(effindex - 1);
                    mapTreeView.SelectedNode.PrevNode.Remove();
                }

                (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects)[effindex] = new Effect
                {
                    Type = EffectType.Safepoint,
                    Coordinate = effCoordTxt.Text,
                    Radius = effRadTxt.Value.ToString(),
                    Prime = isNoChange.Checked ? "11" : GeneratePrimeForEffect().ToString(),
                    NoChange = isNoChange.Checked,
                    PunchEnabled = effPunchEnabled.Checked,
                    SlamEnabled = effSlamEnabled.Checked,
                    PowerblockEnabled = effPowerBlockEnabled.Checked,
                };
                mapTreeView.SelectedNode.Text = "Effect: Safepoint";
                mapTreeView.SelectedNode = mapTreeView.SelectedNode.Parent;
                return;
            }

            if (effTypeComboBox.SelectedItem.ToString() == "Entry Portal")
            {
                if (currentEff.Type != EffectType.EntryPortal)
                {
                    MessageBox.Show("You can't change other effects to an entry portal.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects)[effindex] = new Effect
                {
                    Type = EffectType.EntryPortal,
                    Coordinate = effCoordTxt.Text,
                    Radius = effRadTxt.Value.ToString(),
                    Prime = issEffCdReset.Checked ? ("-" + (isNoChange.Checked ? "11" : GeneratePrimeForEffect().ToString())) : (isNoChange.Checked ? "11" : GeneratePrimeForEffect().ToString()),
                    NoChange = isNoChange.Checked,
                    PunchEnabled = effPunchEnabled.Checked,
                    SlamEnabled = effSlamEnabled.Checked,
                    PowerblockEnabled = effPowerBlockEnabled.Checked,
                    CDReset = issEffCdReset.Checked
                };

                mapTreeView.SelectedNode.Text = "Effect: Entry Portal";
                mapTreeView.SelectedNode = mapTreeView.SelectedNode.Parent;
                effTypeComboBox.Enabled = true;
                return;
            }

            if (effTypeComboBox.SelectedItem.ToString() == "Exit Portal")
            {
                if (currentEff.Type != EffectType.ExitPortal)
                {
                    MessageBox.Show("You can't change other effects to an exit portal.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                (mapcp ? map.levelSelectorCP.Effects : map.Levels[lvlindex].Checkpoints[cpindex].Effects)[effindex] = new Effect
                {
                    Type = EffectType.ExitPortal,
                    Coordinate = effCoordTxt.Text,
                    Radius = effRadTxt.Value.ToString(),
                    Prime = issEffCdReset.Checked ? ("-" + (isNoChange.Checked ? "11" : GeneratePrimeForEffect().ToString())) : (isNoChange.Checked ? "11" : GeneratePrimeForEffect().ToString()),
                    NoChange = isNoChange.Checked,
                    PunchEnabled = effPunchEnabled.Checked,
                    SlamEnabled = effSlamEnabled.Checked,
                    PowerblockEnabled = effPowerBlockEnabled.Checked,
                    CDReset = issEffCdReset.Checked
                };

                mapTreeView.SelectedNode.Text = "Effect: Exit Portal";
                mapTreeView.SelectedNode = mapTreeView.SelectedNode.Parent;
                mapTreeView.Enabled = true;
                effTypeComboBox.Enabled = true;
                return;
            }
        }
    }

    private void effTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        switch (effTypeComboBox.SelectedItem.ToString())
        {
            case "Time":
                timePanel.Visible = true;
                effAbilGroupBox.Enabled = false;
                issEffCdReset.Enabled = false;
                iseffLightShaft.Enabled = true;
                return;
            case "Death":
                timePanel.Visible = false;
                effAbilGroupBox.Enabled = false;
                issEffCdReset.Enabled = false;
                iseffLightShaft.Enabled = true;
                return;
            case "Ability":
                timePanel.Visible = false;
                effAbilGroupBox.Enabled = true;
                issEffCdReset.Enabled = true;
                iseffLightShaft.Enabled = true;
                break;
            case "Permeation":
                timePanel.Visible = false;
                effAbilGroupBox.Enabled = true;
                issEffCdReset.Enabled = true;
                iseffLightShaft.Enabled = true;
                break;
            case "Safepoint":
                timePanel.Visible = false;
                effAbilGroupBox.Enabled = true;
                issEffCdReset.Enabled = false;
                iseffLightShaft.Enabled = false;
                break;
            case "Entry Portal":
                timePanel.Visible = false;
                effAbilGroupBox.Enabled = true;
                issEffCdReset.Enabled = true;
                iseffLightShaft.Enabled = false;
                break;
            case "Exit Portal":
                timePanel.Visible = false;
                effAbilGroupBox.Enabled = true;
                issEffCdReset.Enabled = true;
                iseffLightShaft.Enabled = false;
                break;
        }
    }

    private void addMissBtn_Click(object sender, EventArgs e)
    {
        bool mapcp = false;

        if (addMissBtn.Text == "Add Mission")
        {
            int lvlindex = 1, cpindex = 1;
            if (mapTreeView.SelectedNode.Parent.Text == "Checkpoint 0")
            {
                mapcp = true;
            }

            if (mapTreeView.SelectedNode.Parent.Text != "Checkpoint 0")
            {
                lvlindex = mapTreeView.SelectedNode.Parent.Parent.Index - 1;
                cpindex = mapTreeView.SelectedNode.Parent.Index;
            }

            bool pncfirst = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.RocketPunchFirst);
            bool pwbfirst = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.PowerblockFirst);
            bool slmfirst = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.SlamFirst);

            if (missTypeComboBox.SelectedItem.ToString() == "No Rocket Punch")
            {
                bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.NoRocketPunch);
                if (exists)
                {
                    MessageBox.Show("You've already added the 'No Rocket Punch' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Add(new Mission
                {
                    Type = MissionType.NoRocketPunch,
                    isLock = isMissLock.Checked,
                    isTimeMission = isMissTime.Checked,
                    TimeValue = missTimeUpDown.Value.ToString(),
                });
                mapTreeView.SelectedNode.Nodes.Add("Mission: No Rocket Punch");
                return;
            }

            if (missTypeComboBox.SelectedItem.ToString() == "No Powerblock")
            {
                bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.NoPowerblock);
                if (exists)
                {
                    MessageBox.Show("You've already added the 'No Powerblock' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Add(new Mission
                {
                    Type = MissionType.NoPowerblock,
                    isLock = isMissLock.Checked,
                    isTimeMission = isMissTime.Checked,
                    TimeValue = missTimeUpDown.Value.ToString()
                });
                mapTreeView.SelectedNode.Nodes.Add("Mission: No Powerblock");
            }

            if (missTypeComboBox.SelectedItem.ToString() == "No Seismic Slam")
            {
                bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.NoSlam);
                if (exists)
                {
                    MessageBox.Show("You've already added the 'No Seismic Slam' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Add(new Mission
                {
                    Type = MissionType.NoSlam,
                    isLock = isMissLock.Checked,
                    isTimeMission = isMissTime.Checked,
                    TimeValue = missTimeUpDown.Value.ToString()
                });
                mapTreeView.SelectedNode.Nodes.Add("Mission: No Seismic Slam");
            }

            if (missTypeComboBox.SelectedItem.ToString() == "Stalless")
            {
                bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.Stalless);
                if (exists)
                {
                    MessageBox.Show("You've already added the 'Stalless' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Add(new Mission
                {
                    Type = MissionType.Stalless,
                    isLock = isMissLock.Checked,
                    isTimeMission = isMissTime.Checked,
                    TimeValue = missTimeUpDown.Value.ToString()
                });
                mapTreeView.SelectedNode.Nodes.Add("Mission: Stalless");
            }

            if (missTypeComboBox.SelectedItem.ToString() == "360 Spin")
            {
                bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.Spin);
                if (exists)
                {
                    MessageBox.Show("You've already added the '360 Spin' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Add(new Mission
                {
                    Type = MissionType.Spin,
                    isLock = isMissLock.Checked,
                    isTimeMission = isMissTime.Checked,
                    TimeValue = missTimeUpDown.Value.ToString()
                });
                mapTreeView.SelectedNode.Nodes.Add("Mission: 360 Spin");
            }

            if (missTypeComboBox.SelectedItem.ToString() == "Use Rocket Punch First")
            {
                bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.RocketPunchFirst);
                if (exists)
                {
                    MessageBox.Show("You've already added the 'Use Rocket Punch First' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Add(new Mission
                {
                    Type = MissionType.RocketPunchFirst,
                    isLock = isMissLock.Checked,
                    isTimeMission = isMissTime.Checked,
                    TimeValue = missTimeUpDown.Value.ToString()
                });
                mapTreeView.SelectedNode.Nodes.Add("Mission: Use Rocket Punch First");
            }

            if (missTypeComboBox.SelectedItem.ToString() == "Use Powerblock First")
            {
                bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.PowerblockFirst);
                if (exists)
                {
                    MessageBox.Show("You've already added the 'Use Powerblock First' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Add(new Mission
                {
                    Type = MissionType.PowerblockFirst,
                    isLock = isMissLock.Checked,
                    isTimeMission = isMissTime.Checked,
                    TimeValue = missTimeUpDown.Value.ToString()
                });
                mapTreeView.SelectedNode.Nodes.Add("Mission: Use Powerblock First");
            }

            if (missTypeComboBox.SelectedItem.ToString() == "Use Seismic Slam First")
            {
                bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.SlamFirst);
                if (exists)
                {
                    MessageBox.Show("You've already added the 'Use Seismic Slam First' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Add(new Mission
                {
                    Type = MissionType.SlamFirst,
                    isLock = isMissLock.Checked,
                    isTimeMission = isMissTime.Checked,
                    TimeValue = missTimeUpDown.Value.ToString()
                });
                mapTreeView.SelectedNode.Nodes.Add("Mission: Use Seismic Slam First");
            }

            if (missTypeComboBox.SelectedItem.ToString() == "Upwards Diagonal Punch")
            {
                bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.UpwardsDiag);
                if (exists)
                {
                    MessageBox.Show("You've already added the 'Upwards Diagonal Punch' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Add(new Mission
                {
                    Type = MissionType.UpwardsDiag,
                    isLock = isMissLock.Checked,
                    isTimeMission = isMissTime.Checked,
                    TimeValue = missTimeUpDown.Value.ToString()
                });
                mapTreeView.SelectedNode.Nodes.Add("Mission: Upwards Diagonal Punch");
            }

            if (missTypeComboBox.SelectedItem.ToString() == "Downwards Diagonal Punch")
            {
                bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.DownwardsDiag);
                if (exists)
                {
                    MessageBox.Show("You've already added the 'Downwards Diagonal Punch' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Add(new Mission
                {
                    Type = MissionType.DownwardsDiag,
                    isLock = isMissLock.Checked,
                    isTimeMission = isMissTime.Checked,
                    TimeValue = missTimeUpDown.Value.ToString()
                });
                mapTreeView.SelectedNode.Nodes.Add("Mission: Downwards Diagonal Punch");
            }

            if (missTypeComboBox.SelectedItem.ToString() == "Rocket Punch Bounce")
            {
                bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.PunchBounce);
                if (exists)
                {
                    MessageBox.Show("You've already added the 'Rocket Punch Bounce' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Add(new Mission
                {
                    Type = MissionType.PunchBounce,
                    isLock = isMissLock.Checked,
                    isTimeMission = isMissTime.Checked,
                    TimeValue = missTimeUpDown.Value.ToString()
                });
                mapTreeView.SelectedNode.Nodes.Add("Mission: Rocket Punch Bounce");
            }
        }

        if (addMissBtn.Text == "Edit Mission")
        {
            int lvlindex = 1, cpindex = 1;
            if (mapTreeView.SelectedNode.Parent.Parent.Text == "Checkpoint 0")
            {
                mapcp = true;
            }

            if (mapTreeView.SelectedNode.Parent.Parent.Text != "Checkpoint 0")
            {
                lvlindex = mapTreeView.SelectedNode.Parent.Parent.Parent.Index - 1;
                cpindex = mapTreeView.SelectedNode.Parent.Parent.Index;
            }

            int missindex = int.Parse(mapTreeView.SelectedNode.Tag.ToString());
            var misslist = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions);


            switch (missTypeComboBox.SelectedItem.ToString())
            {
                case "No Rocket Punch":
                    {
                        bool exists = misslist.Any(x => x.Type == MissionType.NoRocketPunch) && misslist[missindex].Type != MissionType.NoRocketPunch;
                        if (exists)
                        {
                            MessageBox.Show("You've already added the 'No Rocket Punch' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        misslist[missindex] = new Mission
                        {
                            Type = MissionType.NoRocketPunch,
                            isLock = isMissLock.Checked,
                            isTimeMission = isMissTime.Checked,
                            TimeValue = missTimeUpDown.Value.ToString(),
                        };
                        mapTreeView.SelectedNode.Text = "Mission: No Rocket Punch";
                        return;
                    }
                case "No Powerblock":
                    {
                        bool exists = misslist.Any(x => x.Type == MissionType.NoPowerblock) && misslist[missindex].Type != MissionType.NoPowerblock;
                        if (exists)
                        {
                            MessageBox.Show("You've already added the 'No Powerblock' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        misslist[missindex] = new Mission
                        {
                            Type = MissionType.NoPowerblock,
                            isLock = isMissLock.Checked,
                            isTimeMission = isMissTime.Checked,
                            TimeValue = missTimeUpDown.Value.ToString()
                        };
                        mapTreeView.SelectedNode.Text = "Mission: No Powerblock";
                        break;
                    }
                case "No Seismic Slam":
                    {
                        bool exists = misslist.Any(x => x.Type == MissionType.NoSlam) && misslist[missindex].Type != MissionType.NoSlam;
                        if (exists)
                        {
                            MessageBox.Show("You've already added the 'No Seismic Slam' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        misslist[missindex] = new Mission
                        {
                            Type = MissionType.NoSlam,
                            isLock = isMissLock.Checked,
                            isTimeMission = isMissTime.Checked,
                            TimeValue = missTimeUpDown.Value.ToString()
                        };
                        mapTreeView.SelectedNode.Text = "Mission: No Seismic Slam";
                        break;
                    }
                case "Stalless":
                    {
                        bool exists = misslist.Any(x => x.Type == MissionType.Stalless) && misslist[missindex].Type != MissionType.Stalless;
                        if (exists)
                        {
                            MessageBox.Show("You've already added the 'Stalless' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        misslist[missindex] = new Mission
                        {
                            Type = MissionType.Stalless,
                            isLock = isMissLock.Checked,
                            isTimeMission = isMissTime.Checked,
                            TimeValue = missTimeUpDown.Value.ToString()
                        };
                        mapTreeView.SelectedNode.Text = "Mission: Stalless";
                        break;
                    }
                case "360 Spin":
                    {
                        bool exists = misslist.Any(x => x.Type == MissionType.Spin) && misslist[missindex].Type != MissionType.Spin;
                        if (exists)
                        {
                            MessageBox.Show("You've already added the '360' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        misslist[missindex] = new Mission
                        {
                            Type = MissionType.Spin,
                            isLock = isMissLock.Checked,
                            isTimeMission = isMissTime.Checked,
                            TimeValue = missTimeUpDown.Value.ToString()
                        };
                        mapTreeView.SelectedNode.Text = "Mission: 360 Spin";
                        break;
                    }
                case "Use Rocket Punch First":
                    {
                        bool exists = misslist.Any(x => x.Type == MissionType.RocketPunchFirst) && misslist[missindex].Type != MissionType.RocketPunchFirst;
                        if (exists)
                        {
                            MessageBox.Show("You've already added the 'Use Rocket Punch First' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        misslist[missindex] = new Mission
                        {
                            Type = MissionType.RocketPunchFirst,
                            isLock = isMissLock.Checked,
                            isTimeMission = isMissTime.Checked,
                            TimeValue = missTimeUpDown.Value.ToString()
                        };
                        mapTreeView.SelectedNode.Text = "Mission: Use Rocket Punch First";
                        break;
                    }
                case "Use Powerblock First":
                    {
                        bool exists = misslist.Any(x => x.Type == MissionType.PowerblockFirst) && misslist[missindex].Type != MissionType.PowerblockFirst;
                        if (exists)
                        {
                            MessageBox.Show("You've already added the 'Use Powerblock First' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        misslist[missindex] = new Mission
                        {
                            Type = MissionType.PowerblockFirst,
                            isLock = isMissLock.Checked,
                            isTimeMission = isMissTime.Checked,
                            TimeValue = missTimeUpDown.Value.ToString()
                        };
                        mapTreeView.SelectedNode.Text = "Mission: Use Powerblock First";
                        break;
                    }
                case "Use Seismic Slam First":
                    {
                        bool exists = misslist.Any(x => x.Type == MissionType.SlamFirst) && misslist[missindex].Type != MissionType.SlamFirst;
                        if (exists)
                        {
                            MessageBox.Show("You've already added the 'Use Seismic Slam First' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        misslist[missindex] = new Mission
                        {
                            Type = MissionType.SlamFirst,
                            isLock = isMissLock.Checked,
                            isTimeMission = isMissTime.Checked,
                            TimeValue = missTimeUpDown.Value.ToString()
                        };
                        mapTreeView.SelectedNode.Text = "Mission: Use Seismic Slam First";
                        break;
                    }
                case "Upwards Diagonal Punch":
                    {
                        bool exists = misslist.Any(x => x.Type == MissionType.UpwardsDiag) && misslist[missindex].Type != MissionType.UpwardsDiag;
                        if (exists)
                        {
                            MessageBox.Show("You've already added the 'Upwards Diagonal Punch' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        misslist[missindex] = new Mission
                        {
                            Type = MissionType.UpwardsDiag,
                            isLock = isMissLock.Checked,
                            isTimeMission = isMissTime.Checked,
                            TimeValue = missTimeUpDown.Value.ToString()
                        };
                        mapTreeView.SelectedNode.Text = "Mission: Upwards Diagonal Punch";
                        break;
                    }
                case "Downwards Diagonal Punch":
                    {
                        bool exists = misslist.Any(x => x.Type == MissionType.DownwardsDiag) && misslist[missindex].Type != MissionType.DownwardsDiag;
                        if (exists)
                        {
                            MessageBox.Show("You've already added the 'Downwards Diagonal Punch' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        misslist[missindex] = new Mission
                        {
                            Type = MissionType.DownwardsDiag,
                            isLock = isMissLock.Checked,
                            isTimeMission = isMissTime.Checked,
                            TimeValue = missTimeUpDown.Value.ToString()
                        };
                        mapTreeView.SelectedNode.Text = "Mission: Downwards Diagonal Punch";
                        break;
                    }
                case "Rocket Punch Bounce":
                    {
                        bool exists = misslist.Any(x => x.Type == MissionType.PunchBounce) && misslist[missindex].Type != MissionType.PunchBounce;
                        if (exists)
                        {
                            MessageBox.Show("You've already added the 'Rocket Punch Bounce' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        misslist[missindex] = new Mission
                        {
                            Type = MissionType.PunchBounce,
                            isLock = isMissLock.Checked,
                            isTimeMission = isMissTime.Checked,
                            TimeValue = missTimeUpDown.Value.ToString()
                        };
                        mapTreeView.SelectedNode.Text = "Mission: Rocket Punch Bounce";
                        break;
                    }
            }
        }
    }

    private void isMissLock_CheckedChanged(object sender, EventArgs e)
    {
        if (isMissLock.Checked)
        {
            missTimeUpDown.Value = 0;
            missTimeUpDown.Enabled = false;
        }

        else
        {
            missTimeUpDown.Value = 0;
            missTimeUpDown.Enabled = true;
        }
    }

    private void saveBtn_Click(object sender, EventArgs e)
    {
        SaveFileDialog sf = new SaveFileDialog();
        sf.Filter = "JSON Files|*.json";
        sf.Title = "Save Map Data as JSON";
        sf.RestoreDirectory = true;
        if (sf.ShowDialog() == DialogResult.OK)
        {
            string json = JsonConvert.SerializeObject(map, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(sf.FileName, json);
            MessageBox.Show("Map Data Saved!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void loadBtn_Click(object sender, EventArgs e)
    {
        OpenFileDialog of = new OpenFileDialog();
        of.Filter = "JSON Files|*.json";
        of.Title = "Load Map Data as JSON";
        of.RestoreDirectory = true;
        if (of.ShowDialog() == DialogResult.OK)
        {
            string json = File.ReadAllText(of.FileName);
            map = JsonConvert.DeserializeObject<Map>(json);
            if (!string.IsNullOrWhiteSpace(map.levelSelectorCP.Coordinate) || !string.IsNullOrEmpty(map.levelSelectorCP.Coordinate))
            {
                mapTreeView.Nodes.Clear();
                mapTreeView.Nodes.Add("Map");
                mapTreeView.Nodes[0].Nodes.Add("Checkpoint 0");
                mapTreeView.Nodes[0].Nodes[0].Nodes.Add("Effects");
                mapTreeView.Nodes[0].Nodes[0].Nodes.Add("Missions");
                if (map.levelSelectorCP.Effects.Count > 0)
                {
                    for (int i = 0; i < map.levelSelectorCP.Effects.Count; i++)
                    {
                        mapTreeView.Nodes[0].Nodes[0].Nodes[0].Nodes.Add(map.levelSelectorCP.Effects[i].ToNodeString());
                    }
                }

                if (map.levelSelectorCP.Missions.Count > 0)
                {
                    for (int i = 0; i < map.levelSelectorCP.Missions.Count; i++)
                    {
                        mapTreeView.Nodes[0].Nodes[0].Nodes[1].Nodes.Add(map.levelSelectorCP.Missions[i].ToNodeString());
                    }
                }
            }

            if (map.Levels.Any())
            {
                for (int i = 0; i < map.Levels.Count; i++)
                {
                    TreeNode lvlnode = new TreeNode();
                    lvlnode.Text = "Level - " + map.Levels[i].Name;
                    lvlnode.Tag = map.Levels[i].Name;
                    mapTreeView.Nodes[0].Nodes.Add(lvlnode);
                    for (int j = 0; j < map.Levels[i].Checkpoints.Count; j++)
                    {
                        if (!map.Levels[i].Color.Contains('('))
                        {
                            map.Levels[i].Color = "Color(" + map.Levels[i].Color + ")";
                        }
                        mapTreeView.Nodes[0].Nodes[i + 1].Nodes.Add("Checkpoint " + (j + 1));
                        if (map.Levels[i].Checkpoints[j].Type != CheckpointType.LevelEnd)
                        {
                            mapTreeView.Nodes[0].Nodes[i + 1].Nodes[j].Nodes.Add("Effects");
                            mapTreeView.Nodes[0].Nodes[i + 1].Nodes[j].Nodes.Add("Missions");
                        }

                        if (map.Levels[i].Checkpoints[j].Effects.Count > 0)
                        {
                            for (int k = 0; k < map.Levels[i].Checkpoints[j].Effects.Count; k++)
                            {
                                mapTreeView.Nodes[0].Nodes[i + 1].Nodes[j].Nodes[0].Nodes.Add(map.Levels[i].Checkpoints[j].Effects[k].ToNodeString());
                            }
                        }

                        if (map.Levels[i].Checkpoints[j].Missions.Count > 0)
                        {
                            for (int k = 0; k < map.Levels[i].Checkpoints[j].Missions.Count; k++)
                            {
                                mapTreeView.Nodes[0].Nodes[i + 1].Nodes[j].Nodes[1].Nodes.Add(map.Levels[i].Checkpoints[j].Missions[k].ToNodeString());
                            }
                        }
                    }
                }
                saveBtn.Enabled = true;
                clipboardLbl.Enabled = true;
            }

            MessageBox.Show("Map Data Loaded!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    public void LoadMap()
    {
        if (!string.IsNullOrWhiteSpace(map.levelSelectorCP.Coordinate) || !string.IsNullOrEmpty(map.levelSelectorCP.Coordinate))
        {
            mapTreeView.Nodes.Clear();
            mapTreeView.Nodes.Add("Map");
            mapTreeView.Nodes[0].Nodes.Add("Checkpoint 0");
            mapTreeView.Nodes[0].Nodes[0].Nodes.Add("Effects");
            mapTreeView.Nodes[0].Nodes[0].Nodes.Add("Missions");
            if (map.levelSelectorCP.Effects.Count > 0)
            {
                for (int i = 0; i < map.levelSelectorCP.Effects.Count; i++)
                {
                    mapTreeView.Nodes[0].Nodes[0].Nodes[0].Nodes.Add(map.levelSelectorCP.Effects[i].ToNodeString());
                }
            }

            if (map.levelSelectorCP.Missions.Count > 0)
            {
                for (int i = 0; i < map.levelSelectorCP.Missions.Count; i++)
                {
                    mapTreeView.Nodes[0].Nodes[0].Nodes[1].Nodes.Add(map.levelSelectorCP.Missions[i].ToNodeString());
                }
            }
        }

        if (map.Levels.Any())
        {
            for (int i = 0; i < map.Levels.Count; i++)
            {
                TreeNode lvlnode = new TreeNode();
                lvlnode.Text = "Level - " + map.Levels[i].Name;
                lvlnode.Tag = map.Levels[i].Name;
                mapTreeView.Nodes[0].Nodes.Add(lvlnode);
                for (int j = 0; j < map.Levels[i].Checkpoints.Count; j++)
                {
                    mapTreeView.Nodes[0].Nodes[i + 1].Nodes.Add("Checkpoint " + (j + 1));
                    if (map.Levels[i].Checkpoints[j].Type != CheckpointType.LevelEnd)
                    {
                        mapTreeView.Nodes[0].Nodes[i + 1].Nodes[j].Nodes.Add("Effects");
                        mapTreeView.Nodes[0].Nodes[i + 1].Nodes[j].Nodes.Add("Missions");
                    }

                    if (map.Levels[i].Checkpoints[j].Effects.Count > 0)
                    {
                        for (int k = 0; k < map.Levels[i].Checkpoints[j].Effects.Count; k++)
                        {
                            mapTreeView.Nodes[0].Nodes[i + 1].Nodes[j].Nodes[0].Nodes.Add(map.Levels[i].Checkpoints[j].Effects[k].ToNodeString());
                        }
                    }

                    if (map.Levels[i].Checkpoints[j].Missions.Count > 0)
                    {
                        for (int k = 0; k < map.Levels[i].Checkpoints[j].Missions.Count; k++)
                        {
                            mapTreeView.Nodes[0].Nodes[i + 1].Nodes[j].Nodes[1].Nodes.Add(map.Levels[i].Checkpoints[j].Missions[k].ToNodeString());
                        }
                    }
                }
            }
            saveBtn.Enabled = true;
            clipboardLbl.Enabled = true;
        }

        MessageBox.Show("Map Data Loaded!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void clipboardLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        Clipboard.SetText(map.GenerateMapData(coordCbox.Checked));
    }

    private void mapTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            if (e.Node.Text == "Map" || e.Node.Text == "Checkpoint 0" || e.Node.Text == "Effects" || e.Node.Text == "Missions")
            {
                return;
            }

            if (e.Node.Text.Contains("Level - "))
            {
                int lvlindex = map.Levels.FindIndex(x => x.Name == e.Node.Tag.ToString());
                moveUpBtn.Visible = true;
                moveDownBtn.Visible = true;
                treeMenuStrip.Items[2].Text = "Delete Level";
                if (map.Levels.Count == 1)
                {
                    moveUpBtn.Visible = false;
                    moveDownBtn.Visible = false;

                    mapTreeView.SelectedNode = e.Node;
                    treeMenuStrip.Show(mapTreeView, e.Location);
                    return;
                }

                if (lvlindex == 0)
                {
                    moveUpBtn.Visible = false;
                }

                else if (lvlindex == map.Levels.Count - 1)
                {
                    moveDownBtn.Visible = false;
                }

                mapTreeView.SelectedNode = e.Node;
                treeMenuStrip.Show(mapTreeView, e.Location);

            }

            if (e.Node.Text.Contains("Checkpoint"))
            {
                moveUpBtn.Visible = true;
                moveDownBtn.Visible = true;
                treeMenuStrip.Items[2].Text = "Delete Checkpoint";
                int lvlindex = e.Node.Parent.Index - 1;
                int cpindex = e.Node.Index;
                CheckpointType checkpointType = map.Levels[lvlindex].Checkpoints[cpindex].Type;

                if (checkpointType == CheckpointType.LevelStart)
                {
                    return;

                }

                if (checkpointType == CheckpointType.LevelEnd)
                {
                    return;
                }

                if (e.Node.Parent.Nodes.Count == 1)
                {
                    moveUpBtn.Visible = false;
                    moveDownBtn.Visible = false;

                    mapTreeView.SelectedNode = e.Node;
                    treeMenuStrip.Show(mapTreeView, e.Location);
                    return;
                }

                if (checkpointType == CheckpointType.Normal)
                {
                    if (!map.Levels[lvlindex].Checkpoints.Any(x => x.Type == CheckpointType.LevelStart) && !map.Levels[lvlindex].Checkpoints.Any(x => x.Type == CheckpointType.LevelEnd))
                    {
                        if (cpindex == 0)
                        {
                            moveUpBtn.Visible = false;
                        }

                        if (cpindex == e.Node.Parent.Nodes.Count - 1)
                        {
                            moveDownBtn.Visible = false;
                        }
                    }

                    if (map.Levels[lvlindex].Checkpoints.Any(x => x.Type == CheckpointType.LevelStart) && map.Levels[lvlindex].Checkpoints[cpindex - 1].Type == CheckpointType.LevelStart)
                    {
                        moveUpBtn.Visible = false;
                        if (cpindex == e.Node.Parent.Nodes.Count - 1)
                        {
                            moveDownBtn.Visible = false;
                        }
                    }

                    if (map.Levels[lvlindex].Checkpoints.Any(x => x.Type == CheckpointType.LevelEnd) && map.Levels[lvlindex].Checkpoints[cpindex + 1].Type == CheckpointType.LevelEnd)
                    {
                        moveDownBtn.Visible = false;
                        mapTreeView.SelectedNode = e.Node;
                        treeMenuStrip.Show(mapTreeView, e.Location);
                        return;
                    }
                }
            }

            if (e.Node.Text.Contains("Effect:"))
            {
                moveUpBtn.Visible = false;
                moveDownBtn.Visible = false;
                treeMenuStrip.Items[2].Text = "Delete Effect";
            }

            if (e.Node.Text.Contains("Mission:"))
            {
                moveUpBtn.Visible = false;
                moveDownBtn.Visible = false;
                treeMenuStrip.Items[2].Text = "Delete Mission";
            }

            mapTreeView.SelectedNode = e.Node;
            treeMenuStrip.Show(mapTreeView, e.Location);
        }
    }

    private void deleteBtn_Click(object sender, EventArgs e)
    {
        if (deleteBtn.Text == "Delete Level")
        {
            if (mapTreeView.SelectedNode.Text.Contains("Level - "))
            {
                string lvlname = mapTreeView.SelectedNode.Tag.ToString();
                mapTreeView.SelectedNode.Remove();
                map.Levels.RemoveAll(x => x.Name == lvlname);
            }
        }

        if (deleteBtn.Text == "Delete Checkpoint")
        {
            if (mapTreeView.SelectedNode.Text.Contains("Checkpoint"))
            {
                string lvlname = mapTreeView.SelectedNode.Parent.Tag.ToString();
                int cpindex = mapTreeView.SelectedNode.Index;
                var lvl = mapTreeView.SelectedNode.Parent;
                mapTreeView.SelectedNode.Remove();
                mapTreeView.SelectedNode = lvl;
                map.Levels.Find(x => x.Name == lvlname).Checkpoints.RemoveAt(cpindex);
                for (int i = 0; i < mapTreeView.SelectedNode.Nodes.Count; i++)
                {
                    mapTreeView.SelectedNode.Nodes[i].Text = "Checkpoint " + (i + 1).ToString();
                }
            }
        }

        if (deleteBtn.Text == "Delete Effect")
        {
            if (mapTreeView.SelectedNode.Text.Contains("Effect:"))
            {
                bool mapcp = mapTreeView.SelectedNode.Parent.Parent.Text == "Checkpoint 0";
                string lvlname = mapcp ? "" : mapTreeView.SelectedNode.Parent.Parent.Parent.Tag.ToString();
                int cpindex = mapTreeView.SelectedNode.Parent.Parent.Index;
                int effectindex = mapTreeView.SelectedNode.Index;
                Effect eff = (mapcp ? map.levelSelectorCP.Effects : map.Levels.Find(x => x.Name == lvlname).Checkpoints[cpindex].Effects)[effectindex];

                if (eff.Type == EffectType.EntryPortal)
                {
                    (mapcp ? map.levelSelectorCP.Effects : map.Levels.Find(x => x.Name == lvlname).Checkpoints[cpindex].Effects).RemoveAt(effectindex);
                    (mapcp ? map.levelSelectorCP.Effects : map.Levels.Find(x => x.Name == lvlname).Checkpoints[cpindex].Effects).RemoveAt(effectindex + 1);
                    mapTreeView.SelectedNode.NextNode.Remove();
                    mapTreeView.SelectedNode.Remove();
                    return;
                }

                if (eff.Type == EffectType.ExitPortal)
                {
                    (mapcp ? map.levelSelectorCP.Effects : map.Levels.Find(x => x.Name == lvlname).Checkpoints[cpindex].Effects).RemoveAt(effectindex);
                    (mapcp ? map.levelSelectorCP.Effects : map.Levels.Find(x => x.Name == lvlname).Checkpoints[cpindex].Effects).RemoveAt(effectindex - 1);
                    mapTreeView.SelectedNode.PrevNode.Remove();
                    mapTreeView.SelectedNode.Remove();
                    return;
                }

                mapTreeView.SelectedNode.Remove();
                (mapcp ? map.levelSelectorCP.Effects : map.Levels.Find(x => x.Name == lvlname).Checkpoints[cpindex].Effects).RemoveAt(effectindex);
                return;
            }
        }

        if (deleteBtn.Text == "Delete Mission")
        {
            if (mapTreeView.SelectedNode.Text.Contains("Mission:"))
            {
                bool mapcp = mapTreeView.SelectedNode.Parent.Parent.Text == "Checkpoint 0";
                string lvlname = mapcp ? "" : mapTreeView.SelectedNode.Parent.Parent.Parent.Tag.ToString();
                int cpindex = mapTreeView.SelectedNode.Parent.Parent.Index;
                int missionindex = mapTreeView.SelectedNode.Index;

                mapTreeView.SelectedNode.Remove();
                (mapcp ? map.levelSelectorCP.Missions : map.Levels.Find(x => x.Name == lvlname).Checkpoints[cpindex].Missions).RemoveAt(missionindex);

            }
        }
    }

    private void loadKneatBtn_Click(object sender, EventArgs e)
    {
        string kneatdata = Clipboard.GetText();
        string cpPosition = "Global.CPposition =";
        Map newMap = new Map();
        int cpPositionIndex = kneatdata.IndexOf(cpPosition);
        if (cpPositionIndex >= 0)
        {
            int startIndex = cpPositionIndex;
            int endIndex = kneatdata.IndexOf(";", startIndex);
            string cpPositionLine = kneatdata.Substring(startIndex, endIndex - startIndex + 1);
            string patternz = @"Vector\(([\d\.\-, ]+)\)";
            MatchCollection checkpoints = Regex.Matches(Regex.Replace(cpPositionLine, @"\s+", string.Empty), patternz);
            mapTreeView.Nodes.Clear();
            mapTreeView.Nodes.Add("Map");
            newMap.TopLeftInfo = GetInfoText();
            var mapsettings = GetMapSettings();
            newMap.SelectedMap = mapsettings[1];
            newMap.Type = mapsettings[0];
            var lvllist = GetLevelNames();
            lvllist.RemoveAt(0);
            for (var index = 0; index < lvllist.Count; index++)
            {
                newMap.Levels.Add(new Level() { Name = lvllist[index], Icon = GetLevelIcon(index), Color = GetLevelColor(index) });
            }

            int lvlindex = -1;
            bool islevelstart = true;
            for (int i = 0; i < checkpoints.Count; i++)
            {
                Match match = checkpoints[i];
                if (i == 0)
                {
                    newMap.levelSelectorCP.Coordinate = checkpoints[i].Groups[1].Value;
                    newMap.levelSelectorCP.Prime = GetPrime(i);
                    var primefactors = int.Parse(newMap.levelSelectorCP.Prime).PrimeFactorization();
                    newMap.levelSelectorCP.PunchEnabled = !primefactors.Any(x => x == 2);
                    newMap.levelSelectorCP.PowerblockEnabled = !primefactors.Any(x => x == 5);
                    newMap.levelSelectorCP.SlamEnabled = !primefactors.Any(x => x == 3);
                    newMap.levelSelectorCP.EffectLock = primefactors.Any(x => x == 17);
                    newMap.levelSelectorCP.Radius = GetRadius(i);
                    var abilitycount = GetAbilityCount(i);
                    newMap.levelSelectorCP.isAbilCount = abilitycount[0] == "False" ? false : true;
                    newMap.levelSelectorCP.Type = CheckpointType.LevelSelector;
                    if (newMap.levelSelectorCP.isAbilCount)
                    {
                        newMap.levelSelectorCP.PunchCount = abilitycount[0];
                        newMap.levelSelectorCP.PowerblockCount = abilitycount[1];
                        newMap.levelSelectorCP.SlamCount = abilitycount[2];
                    }
                    var efflist = GetEffects(i);
                    if (efflist.Any())
                    {
                        for (int i1 = 0; i1 < efflist.Count; i1++)
                        {
                            newMap.levelSelectorCP.Effects.Add(efflist[i1]);
                        }
                    }
                    var misslist = GetMissions(i);
                    if (misslist.Any())
                    {
                        for (int i1 = 0; i1 < misslist.Count; i1++)
                        {
                            newMap.levelSelectorCP.Missions.Add(misslist[i1]);
                        }
                    }
                }

                else
                {
                    bool islvlEnd = GetPrime(i + 1) == null || int.Parse(GetPrime(i + 1)).PrimeFactorization().Any(x => x == 13);
                    islevelstart = int.Parse(GetPrime(i)).PrimeFactorization().Any(x => x == 13);
                    if (islevelstart)
                    {
                        lvlindex++;
                    }

                    if (islvlEnd == false)
                    {
                        var primefactors = int.Parse(GetPrime(i)).PrimeFactorization();

                        Checkpoint newcp = new Checkpoint()
                        {
                            Coordinate = checkpoints[i].Groups[1].Value,
                            Prime = GetPrime(i),
                            Radius = GetRadius(i),
                            TeleportCoordinate = GetTP(i) == "False" ? "" : GetTP(i),
                            TeleportRadius = GetTP(i) == "False" ? "2" : GetTPRadius(i),
                            EffectLock = primefactors.Any(x => x == 17),
                            PunchEnabled = !primefactors.Any(x => x == 2),
                            PowerblockEnabled = !primefactors.Any(x => x == 5),
                            SlamEnabled = !primefactors.Any(x => x == 3),
                            Type = primefactors.Any(x => x == 13) ? CheckpointType.LevelStart : CheckpointType.Normal
                        };

                        var abilitycount = GetAbilityCount(i);
                        newcp.isAbilCount = abilitycount[0] == "False" ? false : true;

                        if (newcp.isAbilCount)
                        {
                            newcp.PunchCount = abilitycount[0];
                            newcp.PowerblockCount = abilitycount[1];
                            newcp.SlamCount = abilitycount[2];
                        }

                        var efflist = GetEffects(i);
                        if (efflist.Any())
                        {
                            efflist.ForEach(x => newcp.Effects.Add(x));
                        }

                        var misslist = GetMissions(i);
                        if (efflist.Any())
                        {
                            misslist.ForEach(x => newcp.Missions.Add(x));
                        }
                        newMap.Levels[lvlindex].Checkpoints.Add(newcp);
                    }

                    if (islvlEnd)
                    {
                        Checkpoint newcp = new Checkpoint()
                        {
                            Coordinate = checkpoints[i].Groups[1].Value,
                            Prime = "True",
                            Radius = GetRadius(i),
                            Type = CheckpointType.LevelEnd
                        };
                        newMap.Levels[lvlindex].Checkpoints.Add(newcp);
                    }
                }
            }
            map = newMap;
            LoadMap();
            mapTreeView.Nodes[0].Expand();
        }
    }

    public string GetInfoText()
    {
        string fileContent = Clipboard.GetText();
        string target = "Global.InfoText = Custom String(\"";
        int targetIndex = fileContent.IndexOf(target) + target.Length;
        if (targetIndex >= 0)
        {
            int startIndex = targetIndex;
            int endIndex = fileContent.IndexOf("\");", startIndex);
            string targetLine = fileContent.Substring(startIndex, endIndex - startIndex);
            return targetLine;
        }
        return null;
    }

    public string[] GetMapSettings()
    {
        string input = Clipboard.GetText();
        string pattern = @"(\w+)\s*\{\s*enabled maps\s*\{\s*(.*?)\s*\}\s*\}";
        MatchCollection matches = Regex.Matches(input, pattern);

        foreach (Match match in matches)
        {
            if (match.Groups[2].Value.Trim() != "")
            {
                return new string[] { match.Groups[1].Value, match.Groups[2].Value.Replace("0", string.Empty).Trim() }; //0 = map type, 1 = selected map
            }
        }
        return null;
    }

    public List<string> GetLevelNames()
    {
        string fileContent = Clipboard.GetText();
        string target = "Global.LvlName = Array(";
        int targetIndex = fileContent.IndexOf(target) + target.Length;
        if (targetIndex >= 0)
        {
            int startIndex = targetIndex;
            int endIndex = fileContent.IndexOf(");", startIndex);
            string targetLine = fileContent.Substring(startIndex, endIndex - startIndex).Replace("\t", string.Empty);
            targetLine = Regex.Replace(targetLine, @"\r\n?|\n", string.Empty);
            string patternz = @"Custom\s+String\s*\(""(.*?)""\s*\)";
            MatchCollection matches = Regex.Matches(targetLine, patternz);
            return (from Match m in matches select m.Groups[1].Value).ToList();
        }
        return null;
    }

    public string GetLevelColor(int index)
    {
        string fileContent = Clipboard.GetText();
        string target = "Global.LvlColors = Array(";
        int targetIndex = fileContent.IndexOf(target) + target.Length;
        if (targetIndex >= 0)
        {
            int startIndex = targetIndex;
            int endIndex = fileContent.IndexOf(");", startIndex);
            string targetLine = fileContent.Substring(startIndex, endIndex - startIndex).Replace("\t", string.Empty);
            targetLine = Regex.Replace(targetLine, @"\r\n?|\n", string.Empty);
            string patternz = @"(?:Custom\s)?Color\(([\w, ]+)\)";
            MatchCollection matches = Regex.Matches(targetLine, patternz);
            //string colorValue = matches[index + 1].Groups[1].Value;
            /*if (matches[index + 1].Groups[1].Value == "Custom" || colorValue.Split(',').Length >= 2)
            {
                return "White";
            }*/
            return matches[index + 1].Value;
        }
        return null;
    }

    public string GetLevelIcon(int index)
    {
        string fileContent = Clipboard.GetText();
        string target = "Skip(-2 + Global.LevelCounter * 2);";
        int targetIndex = fileContent.IndexOf(target) + target.Length;
        if (targetIndex >= 0)
        {
            int startIndex = targetIndex;
            int endIndex = fileContent.IndexOf("}", startIndex);
            string targetLine = fileContent.Substring(startIndex, endIndex - startIndex).Replace("\t", string.Empty).Replace("\r\n", "");
            string patternz = @"Up \* [0-9]+\.[0-9]+,[ ]*(.*?), Visible To( and Color)?,";
            MatchCollection matches = Regex.Matches(targetLine, patternz);
            return matches[index].Groups[1].Value;
        }
        return null;
    }

    public string GetPrime(int index)
    {
        string fileContent = Clipboard.GetText();
        string target = "Global.Prime =";
        int targetIndex = fileContent.IndexOf(target) + target.Length;
        if (targetIndex >= 0)
        {
            int startIndex = targetIndex;
            int endIndex = fileContent.IndexOf(";", startIndex);
            string targetLine = fileContent.Substring(startIndex, endIndex - startIndex + 1)
                .Replace("Array(", string.Empty)
                .Replace(");", string.Empty)
                .Replace("True", "1");
            targetLine = Regex.Replace(targetLine, @"\s+", string.Empty);
            return targetLine.Split(',').Length <= index ? null : targetLine.Split(',')[index];
        }
        return null;
    }

    public string GetRadius(int index)
    {
        string fileContent = Clipboard.GetText();
        string target = "Global.Radius_VA_GoBackCP =";
        int targetIndex = fileContent.IndexOf(target);
        if (targetIndex >= 0)
        {
            int startIndex = targetIndex;
            int endIndex = fileContent.IndexOf(";", startIndex);
            string targetLine = fileContent.Substring(startIndex, endIndex - startIndex + 1);
            string patternz = @"Vector\(([\d\.\-, ]+)\)";
            MatchCollection matches = Regex.Matches(Regex.Replace(targetLine, @"\s+", string.Empty), patternz);
            return matches[index].Groups[1].Value.Split(',')[0];
        }
        return "2";
    }

    public string GetTPRadius(int index)
    {
        string fileContent = Clipboard.GetText();
        string target = "Global.HiddenCP_TpRad_TT =";
        int targetIndex = fileContent.IndexOf(target) + target.Length;
        if (targetIndex >= 0)
        {
            int startIndex = targetIndex;
            int endIndex = fileContent.IndexOf(";", startIndex);
            string targetLine = fileContent.Substring(startIndex, endIndex - startIndex + 1)
                .Replace("Array(", string.Empty)
                .Replace(");", string.Empty);
            string patternz = @"False|Vector\(([\d\.\-, ]+)\)";
            MatchCollection matches = Regex.Matches(Regex.Replace(targetLine, @"\s+", string.Empty), patternz);
            if (matches[index].Value == "False")
            {
                return "False";
            }
            return matches[index].Groups[1].Value.Split(',')[1];
        }
        return "";
    }

    public string[] GetAbilityCount(int index)
    {
        string fileContent = Clipboard.GetText();
        string target = "Global.AbilityCount =";
        int targetIndex = fileContent.IndexOf(target) + target.Length;
        if (targetIndex >= 0)
        {
            int startIndex = targetIndex;
            int endIndex = fileContent.IndexOf(";", startIndex);
            string targetLine = fileContent.Substring(startIndex, endIndex - startIndex + 1)
                .Replace("Array(", string.Empty)
                .Replace(");", string.Empty);
            string patternz = @"0|False|Vector\(([\d\.\-, ]+)\)";
            MatchCollection matches = Regex.Matches(Regex.Replace(targetLine, @"\s+", string.Empty), patternz);
            if (matches[index].Value is "False" or "0") return new string[] { "False" };

            var abilities = matches[index].Groups[1].Value.Split(',');
            return new string[] { abilities[0], abilities[1], abilities[2] };
        }
        return null;
    }

    public string GetTP(int index)
    {
        string fileContent = Clipboard.GetText();
        string target = "Global.TP =";
        int targetIndex = fileContent.IndexOf(target) + target.Length;
        if (targetIndex >= 0)
        {
            int startIndex = targetIndex;
            int endIndex = fileContent.IndexOf(";", startIndex);
            string targetLine = fileContent.Substring(startIndex, endIndex - startIndex + 1)
                .Replace("Array(", string.Empty)
                .Replace(");", string.Empty);
            string patternz = @"0|False|Vector\(([\d\.\-, ]+)\)";
            MatchCollection matches = Regex.Matches(Regex.Replace(targetLine, @"\s+", string.Empty), patternz);
            if (matches[index].Value is "False" or "0")
                return "False";
            return matches[index].Groups[1].Value;
        }
        return "hmm"; //XD
    }

    public int GetConnection(int index)
    {
        string fileContent = Clipboard.GetText();
        string target = "Global.Connections =";
        int targetIndex = fileContent.IndexOf(target) + target.Length;
        if (targetIndex >= 0)
        {
            int startIndex = targetIndex;
            int endIndex = fileContent.IndexOf(");", startIndex);
            string targetLine = fileContent.Substring(startIndex, endIndex - startIndex)
                .Replace("Vector(0,", string.Empty)
                .Replace("0),", string.Empty)
                .Replace("Array(", string.Empty)
                .Replace(");", string.Empty)
                .Replace("False", "0");
            targetLine = Regex.Replace(targetLine, @"\s+", string.Empty);
            return int.Parse(targetLine.Split(',')[index]);
        }
        return -1;
    }

    public List<Effect> GetEffects(int index)
    {
        string fileContent = Clipboard.GetText();
        string target = "Global.Effect = Array(";
        int targetIndex = fileContent.IndexOf(target) + target.Length;
        if (targetIndex >= 0)
        {
            int startIndex = targetIndex;
            int endIndex = fileContent.IndexOf(";", startIndex);
            string targetLine = fileContent.Substring(startIndex, endIndex - startIndex + 1);
            targetLine = Regex.Replace(targetLine, @"\s+", string.Empty);
            string edited = targetLine.Replace("(", "[").Replace(")", "]").Replace("];", string.Empty).Replace("False", "0");
            List<string> values = new List<string>();
            string currentString = "";
            int openBracketCount = 0;
            foreach (char c in edited)
            {
                if (c == ',' && openBracketCount == 0)
                {
                    values.Add(currentString);
                    currentString = "";
                }
                else
                {
                    if (c == '[') openBracketCount++;
                    else if (c == ']') openBracketCount--;
                    currentString += c;
                }
            }
            values.Add(currentString);
            List<Effect> effects = new List<Effect>();
            if (values[index] == "0") return effects;

            MatchCollection mc = Regex.Matches(values[index], @"[Vector[[0-9.,-]+],-?\d+(.\d+)?,\d+,-?\d+]");
            if (mc.Count > 0)
            {
                foreach (Match match in mc)
                {
                    string matchString = match.Value;
                    string[] parts = matchString.Split(new char[] { '[', ']', ',' },
                        StringSplitOptions.RemoveEmptyEntries);
                    if (int.Parse(parts[5]) < 7)
                    {
                        Effect newEffect = new Effect
                        {
                            Coordinate = $"{parts[1]},{parts[2]},{parts[3]}",
                            Radius = parts[4],
                            Lightshaft = parts[4].Contains('-'),
                            Type = (EffectType)int.Parse(parts[5])
                        };
                        newEffect.Prime =
                            (newEffect.Type == EffectType.Time || newEffect.Type == EffectType.Death)
                                ? "1"
                                : parts[6];
                        if (newEffect.Type == EffectType.Time)
                        {
                            newEffect.TimeValue = parts[6];
                            effects.Add(newEffect);
                        }

                        if (newEffect.Type == EffectType.Death)
                        {
                            effects.Add(newEffect);
                        }

                        if (newEffect.Type != EffectType.Time && newEffect.Type != EffectType.Death)
                        {
                            var primeFactors = Math.Abs(int.Parse(parts[6])).PrimeFactorization().Distinct();
                            newEffect.CDReset = parts[6].Contains('-');
                            newEffect.NoChange = primeFactors.Any(x => x == 11);
                            newEffect.PunchEnabled = primeFactors.Any(x => x == 2);
                            newEffect.SlamEnabled = primeFactors.Any(x => x == 3);
                            newEffect.PowerblockEnabled = primeFactors.Any(x => x == 5);
                            effects.Add(newEffect);
                        }
                    }
                }
            }

            return effects;
        }
        return null;
    }

    public List<Mission> GetMissions(int index)
    {
        string fileContent = Clipboard.GetText();
        string target = "Global.Mission = Array(";
        int targetIndex = fileContent.IndexOf(target) + target.Length;
        if (targetIndex >= 0)
        {
            int startIndex = targetIndex;
            int endIndex = fileContent.IndexOf(";", startIndex);
            string targetLine = fileContent.Substring(startIndex, endIndex - startIndex + 1);
            targetLine = Regex.Replace(targetLine, @"\s+", string.Empty);
            string edited = targetLine.Replace("(", "[").Replace(")", "]").Replace("];", string.Empty).Replace("Array", string.Empty);
            List<string> values = new List<string>();
            string currentString = "";
            int openBracketCount = 0;
            foreach (char c in edited)
            {
                if (c == ',' && openBracketCount == 0)
                {
                    values.Add(currentString);
                    currentString = "";
                }
                else
                {
                    if (c == '[') openBracketCount++;
                    else if (c == ']') openBracketCount--;
                    currentString += c;
                }
            }
            values.Add(currentString);
            //values.ForEach(x => richTextBox1.Text += x + Environment.NewLine);
            List<Mission> missions = new List<Mission>();
            if (values[index] == "True") return missions;

            string[] missionValues = values[index]
                .Split(new char[] { '[', ']', ',' }, StringSplitOptions.RemoveEmptyEntries);
            int prime = int.Parse(missionValues[0]);
            var primeFactors = prime.PrimeFactorization().Distinct();

            int i = 1;
            foreach (var primeFactor in primeFactors)
            {
                if (primeFactor != 11)
                {
                    Mission newMission = new Mission();
                    newMission.Type = (MissionType)primeFactor;
                    double missionEffect = double.Parse(missionValues[i]);
                    if (missionEffect >= 9930)
                    {
                        newMission.isLock = true;
                        missions.Add(newMission);
                    }

                    else
                    {
                        newMission.isTimeMission = true;
                        newMission.TimeValue = missionEffect.ToString();
                        missions.Add(newMission);
                    }
                }

                i++;
            }

            return missions;
        }
        return null;
    }

    private void aboutLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        MessageBox.Show("Made for making map data generation easier in the absence of inspector. " +
            "Big thanks to dreadowl, this would be IMPOSSIBLE without his help. - Dorian", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        CheckForUpdates();
        FillMapComboBox();
    }

    public void FillMapComboBox()
    {
        string[] maplist = Properties.Resources.maps.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        for (var index = 0; index < maplist.Length; index++)
        {
            string m = maplist[index];
            mapComboBox.Items.Add(m.Substring(0, m.IndexOf(',')));
            //mapComboBox.Items.Add(m.Substring(m.LastIndexOf(',') + 1));
        }

        mapComboBox.SelectedIndex = 0;
    }

    private void editMapSettingsBtn_Click(object sender, EventArgs e)
    {
        string[] maplist = Properties.Resources.maps.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        if (!string.IsNullOrEmpty(topleftTxt.Text) || !string.IsNullOrWhiteSpace(topleftTxt.Text))
        {
            map.TopLeftInfo = topleftTxt.Text;
        }
        map.SelectedMap = mapComboBox.Text;
        map.Type = maplist[mapComboBox.SelectedIndex].Substring(maplist[mapComboBox.SelectedIndex].LastIndexOf(',') + 1);
    }

    #endregion
    public async void CheckForUpdates()
    {
        var client = new GitHubClient(new ProductHeaderValue("brugmapdataultimate"));
        var releases = await client.Repository.Release.GetAll("Fujimuji", "brugmapdataultimate");
        if (releases[0].TagName != version)
        {
            var result = MessageBox.Show("There is a new version available. Would you like to download it?",
                "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                using (var htclient = new HttpClient())
                {
                    using (var s = htclient.GetStreamAsync(releases[0].Assets
                               .FirstOrDefault(x => x.Name == "brugmapdataultimate.exe").BrowserDownloadUrl))
                    {
                        using (var fs = new FileStream($"brugmapdataultimate{releases[0].TagName}.exe",
                                   FileMode.OpenOrCreate))
                        {
                            s.Result.CopyTo(fs);
                        }
                    }
                }

                MessageBox.Show($"Download complete. Please run the new version {releases[0].TagName}", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (saveBtn.Enabled)
        {
            var result = MessageBox.Show("Would you like to save your changes?", "Save", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (result == DialogResult.Yes) saveBtn.PerformClick();
        }
    }

    private void moveUpBtn_Click(object sender, EventArgs e)
    {
        if (mapTreeView.SelectedNode.Text.Contains("Level -"))
        {
            var lvlindex = map.Levels.FindIndex(x => x.Name == mapTreeView.SelectedNode.Tag.ToString());
            var currentlvl = map.Levels[lvlindex];
            map.Levels.RemoveAt(lvlindex);
            map.Levels.Insert(lvlindex - 1, currentlvl);
            var currentlvlnode = mapTreeView.SelectedNode;
            var currentlvlnodeindex = mapTreeView.SelectedNode.Index;
            var prevlvlnode = mapTreeView.SelectedNode.PrevNode;
            mapTreeView.SelectedNode.Remove();
            mapTreeView.Nodes[0].Nodes.Insert(currentlvlnodeindex - 1, currentlvlnode);
            mapTreeView.SelectedNode = currentlvlnode;
        }

        if (mapTreeView.SelectedNode.Text.Contains("Checkpoint"))
        {
            var cpindex = mapTreeView.SelectedNode.Index;
            var currentcp = map.Levels.Find(x => x.Name == mapTreeView.SelectedNode.Parent.Tag.ToString())
                .Checkpoints[cpindex];

            map.Levels.Find(x => x.Name == mapTreeView.SelectedNode.Parent.Tag.ToString()).Checkpoints
                .RemoveAt(cpindex);
            map.Levels.Find(x => x.Name == mapTreeView.SelectedNode.Parent.Tag.ToString()).Checkpoints
                .Insert(cpindex - 1, currentcp);
            mapTreeView.SelectedNode = mapTreeView.SelectedNode.Parent;
            mapTreeView.SelectedNode = mapTreeView.SelectedNode.Nodes[cpindex - 1];
        }
    }

    private void moveDownBtn_Click(object sender, EventArgs e)
    {
        if (mapTreeView.SelectedNode.Text.Contains("Level -"))
        {
            var lvlindex = map.Levels.FindIndex(x => x.Name == mapTreeView.SelectedNode.Tag.ToString());
            var currentlvl = map.Levels[lvlindex];
            map.Levels.RemoveAt(lvlindex);
            map.Levels.Insert(lvlindex + 1, currentlvl);
            var currentlvlnode = mapTreeView.SelectedNode;
            var currentlvlnodeindex = mapTreeView.SelectedNode.Index;
            var prevlvlnode = mapTreeView.SelectedNode.PrevNode;
            mapTreeView.SelectedNode.Remove();
            mapTreeView.Nodes[0].Nodes.Insert(currentlvlnodeindex + 1, currentlvlnode);
            mapTreeView.SelectedNode = currentlvlnode;
        }

        if (mapTreeView.SelectedNode.Text.Contains("Checkpoint"))
        {
            var cpindex = mapTreeView.SelectedNode.Index;
            var currentcp = map.Levels.Find(x => x.Name == mapTreeView.SelectedNode.Parent.Tag.ToString())
                .Checkpoints[cpindex];

            map.Levels.Find(x => x.Name == mapTreeView.SelectedNode.Parent.Tag.ToString()).Checkpoints
                .RemoveAt(cpindex);
            map.Levels.Find(x => x.Name == mapTreeView.SelectedNode.Parent.Tag.ToString()).Checkpoints
                .Insert(cpindex + 1, currentcp);
            mapTreeView.SelectedNode = mapTreeView.SelectedNode.Parent;
            mapTreeView.SelectedNode = mapTreeView.SelectedNode.Nodes[cpindex + 1];
        }
    }
}

public static class PrimeExtension
{
    public static List<int> PrimeFactorization(this int n)
    {
        List<int> factors = new List<int>();
        for (int i = 2; i <= n / i; i++)
        {
            while (n % i == 0)
            {
                factors.Add(i);
                n /= i;
            }
        }
        if (n > 1)
        {
            factors.Add(n);
        }
        return factors;
    }
}

public static class EffectExtensions
{
    public static string ToNodeString(this Effect me)
    {
        return me.Type switch
        {
            EffectType.Time => "Effect: Time " + (me.Lightshaft ? "Lightshaft - " : "Orb - ") + me.TimeValue,
            EffectType.Death => "Effect: Death " + (me.Lightshaft ? "Lightshaft" : "Orb"),
            EffectType.Ability => "Effect: Ability " + (me.Lightshaft ? "Lightshaft" : "Orb"),
            EffectType.Permeation => "Effect: Permeation " + (me.Lightshaft ? "Lightshaft" : "Orb"),
            EffectType.Safepoint => "Effect: Safepoint",
            EffectType.EntryPortal => "Effect: Entry Portal",
            EffectType.ExitPortal => "Effect: Exit Portal"
        };
    }
}

public static class MissionExtensions
{
    public static string ToNodeString(this Mission me)
    {
        return me.Type switch
        {
            MissionType.NoRocketPunch => "Mission: No Rocket Punch",
            MissionType.NoSlam => "Mission: No Seismic Slam",
            MissionType.NoPowerblock => "Mission: No Powerblock",
            MissionType.Stalless => "Mission: Stalless",
            MissionType.Spin => "Mission: 360 Spin",
            MissionType.RocketPunchFirst => "Mission: Rocket Punch First",
            MissionType.PowerblockFirst => "Mission: Powerblock First",
            MissionType.SlamFirst => "Mission: Seismic Slam First",
            MissionType.PunchBounce => "Mission: Rocket Punch Bounce"
        };
    }
}