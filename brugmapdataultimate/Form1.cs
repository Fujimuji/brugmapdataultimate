using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static brugmapdataultimate.Form1;

namespace brugmapdataultimate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            effTypeComboBox.SelectedIndex = 0;
            missTypeComboBox.SelectedIndex = 0;
            mapTreeView.SelectedNode = mapTreeView.Nodes[0];
            missSettingsGroupBox.Location = new Point(368, 26);
            effSettingsGroupBox.Location = new Point(368, 26);
            cpSettingsGroupBox.Location = new Point(368, 26);
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

        public enum AbilityPrimes
        {
            Punch = 2,
            Slam = 3,
            Powerblock = 5
        }

        public enum EffectPrimes
        {
            Punch = 2,
            Slam = 3,
            Powerblock = 5,
            NoChange = 11
        }

        public class Map
        {
            public Map()
            {
                levelSelectorCP = new Checkpoint();
                Levels = new List<Level>();
            }
            public Checkpoint levelSelectorCP { get; set; }
            public List<Level> Levels { get; set; }

            public string GenerateMapData()
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
                genRadVAGobackCP += $"Vector({levelSelectorCP.Radius},0,-1)";
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
                        genRadVAGobackCP += "," + $"Vector({cp.Radius},0," + (cp.Type == CheckpointType.LevelStart ? "0" : (cpcount - 1).ToString()) + ")";
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

                return genCPposition + Environment.NewLine +
                       genPrime + Environment.NewLine +
                       genRadVAGobackCP + Environment.NewLine +
                       genConnections + Environment.NewLine +
                       genMission + Environment.NewLine +
                       genHiddenCPTpRadTT + Environment.NewLine +
                       genTP + Environment.NewLine +
                       genEffect + Environment.NewLine +
                       genAbilityCount;
            }
        }

        public class Level
        {
            public string Name { get; set; } = "";
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
                if (TeleportCoordinate == "")
                {
                    return "False";
                }

                else
                {
                    return $"Vector({TeleportCoordinate})";
                }
            }

            public string HiddenCPTPRadTTStringForCP()
            {
                if (TeleportCoordinate == "")
                {
                    return "False";
                }

                else
                {
                    return $"Vector(0,{TeleportRadius},0)";
                }
            }
            public string MissionStringForCP()
            {
                if (!Missions.Any())
                {
                    return "True";
                }

                string genMission = "Array(";
                int _prime = 1;
                int deflock = 9930;

                for (int i = 0; i < Missions.Count; i++)
                {
                    _prime *= (int)Missions[i].Type;
                }

                int currentcount = 1;
                int listcount = Missions.Count;
                genMission += _prime.ToString() + ",";
                if (Missions.Any(x => (int)x.Type == 2))
                {
                    Mission m = Missions.FirstOrDefault(x => (int)x.Type == 2);
                    if (m.isTimeMission)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += m.TimeValue + ")";
                        }

                        else
                        {
                            genMission += m.TimeValue + ",";
                        }
                        currentcount++;
                    }

                    if (m.isLock)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += deflock.ToString() + ")";
                        }

                        else
                        {
                            genMission += deflock.ToString() + ",";
                        }
                        currentcount++;
                        deflock++;
                    }
                }
                if (Missions.Any(x => (int)x.Type == 3))
                {
                    Mission m = Missions.FirstOrDefault(x => (int)x.Type == 3);
                    if (m.isTimeMission)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += m.TimeValue + ")";
                        }

                        else
                        {
                            genMission += m.TimeValue + ",";
                        }
                        currentcount++;
                    }

                    if (m.isLock)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += deflock.ToString() + ")";
                        }

                        else
                        {
                            genMission += deflock.ToString() + ",";
                        }
                        currentcount++;
                        deflock++;
                    }
                }
                if (Missions.Any(x => (int)x.Type == 5))
                {
                    Mission m = Missions.FirstOrDefault(x => (int)x.Type == 5);
                    if (m.isTimeMission)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += m.TimeValue + ")";
                        }

                        else
                        {
                            genMission += m.TimeValue + ",";
                        }
                        currentcount++;
                    }

                    if (m.isLock)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += deflock.ToString() + ")";
                        }

                        else
                        {
                            genMission += deflock.ToString() + ",";
                        }
                        currentcount++;
                        deflock++;
                    }
                }
                if (Missions.Any(x => (int)x.Type == 7))
                {
                    Mission m = Missions.FirstOrDefault(x => (int)x.Type == 7);
                    if (m.isTimeMission)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += m.TimeValue + ")";
                        }

                        else
                        {
                            genMission += m.TimeValue + ",";
                        }
                        currentcount++;
                    }

                    if (m.isLock)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += deflock.ToString() + ")";
                        }

                        else
                        {
                            genMission += deflock.ToString() + ",";
                        }
                        currentcount++;
                        deflock++;
                    }
                }
                if (Missions.Any(x => (int)x.Type == 13))
                {
                    Mission m = Missions.FirstOrDefault(x => (int)x.Type == 13);
                    if (m.isTimeMission)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += m.TimeValue + ")";
                        }

                        else
                        {
                            genMission += m.TimeValue + ",";
                        }
                        currentcount++;
                    }

                    if (m.isLock)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += deflock.ToString() + ")";
                        }

                        else
                        {
                            genMission += deflock.ToString() + ",";
                        }
                        currentcount++;
                        deflock++;
                    }
                }
                if (Missions.Any(x => (int)x.Type == 17))
                {
                    Mission m = Missions.FirstOrDefault(x => (int)x.Type == 17);
                    if (m.isTimeMission)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += m.TimeValue + ")";
                        }

                        else
                        {
                            genMission += m.TimeValue + ",";
                        }
                        currentcount++;
                    }

                    if (m.isLock)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += deflock.ToString() + ")";
                        }

                        else
                        {
                            genMission += deflock.ToString() + ",";
                        }
                        currentcount++;
                        deflock++;
                    }
                }
                if (Missions.Any(x => (int)x.Type == 19))
                {
                    Mission m = Missions.FirstOrDefault(x => (int)x.Type == 19);
                    if (m.isTimeMission)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += m.TimeValue + ")";
                        }

                        else
                        {
                            genMission += m.TimeValue + ",";
                        }
                        currentcount++;
                    }

                    if (m.isLock)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += deflock.ToString() + ")";
                        }

                        else
                        {
                            genMission += deflock.ToString() + ",";
                        }
                        currentcount++;
                        deflock++;
                    }
                }
                if (Missions.Any(x => (int)x.Type == 23))
                {
                    Mission m = Missions.FirstOrDefault(x => (int)x.Type == 23);
                    if (m.isTimeMission)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += m.TimeValue + ")";
                        }

                        else
                        {
                            genMission += m.TimeValue + ",";
                        }
                        currentcount++;
                    }

                    if (m.isLock)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += deflock.ToString() + ")";
                        }

                        else
                        {
                            genMission += deflock.ToString() + ",";
                        }
                        currentcount++;
                        deflock++;
                    }
                }
                if (Missions.Any(x => (int)x.Type == 29))
                {
                    Mission m = Missions.FirstOrDefault(x => (int)x.Type == 29);
                    if (m.isTimeMission)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += m.TimeValue + ")";
                        }

                        else
                        {
                            genMission += m.TimeValue + ",";
                        }
                        currentcount++;
                    }

                    if (m.isLock)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += deflock.ToString() + ")";
                        }

                        else
                        {
                            genMission += deflock.ToString() + ",";
                        }
                        currentcount++;
                        deflock++;
                    }
                }
                if (Missions.Any(x => (int)x.Type == 31))
                {
                    Mission m = Missions.FirstOrDefault(x => (int)x.Type == 31);
                    if (m.isTimeMission)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += m.TimeValue + ")";
                        }

                        else
                        {
                            genMission += m.TimeValue + ",";
                        }
                        currentcount++;
                    }

                    if (m.isLock)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += deflock.ToString() + ")";
                        }

                        else
                        {
                            genMission += deflock.ToString() + ",";
                        }
                        currentcount++;
                        deflock++;
                    }
                }
                if (Missions.Any(x => (int)x.Type == 37))
                {
                    Mission m = Missions.FirstOrDefault(x => (int)x.Type == 37);
                    if (m.isTimeMission)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += m.TimeValue + ")";
                        }

                        else
                        {
                            genMission += m.TimeValue + ",";
                        }
                        currentcount++;
                    }

                    if (m.isLock)
                    {
                        if (currentcount == listcount)
                        {
                            genMission += deflock.ToString() + ")";
                        }

                        else
                        {
                            genMission += deflock.ToString() + ",";
                        }
                        currentcount++;
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
                    if (i == Effects.Count - 1)
                    {
                        genEffect += Effects[i].EffectString() + ")";
                    }

                    else
                    {
                        genEffect += Effects[i].EffectString() + ",";
                    }
                }
                return genEffect;
            }
            public string AbilityCountStringForCP()
            {
                if (isAbilCount == false)
                {
                    return "False";
                }

                else
                {
                    return $"Vector({PunchCount},{PowerblockCount},{SlamCount})";
                }
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
                if (Type == EffectType.Time)
                {
                    return $"Array(Vector({Coordinate}), {Radius}, 0, {TimeValue})";
                }

                if (Type == EffectType.Death)
                {
                    return $"Array(Vector({Coordinate}), {Radius}, 1, 1)";
                }

                if (Type == EffectType.Ability)
                {
                    return $"Array(Vector({Coordinate}), {Radius}, 2, {Prime})";
                }

                if (Type == EffectType.Permeation)
                {
                    return $"Array(Vector({Coordinate}), {Radius}, 3, {Prime})";
                }

                if (Type == EffectType.Safepoint)
                {
                    return $"Array(Vector({Coordinate}), {Radius}, 4, {Prime})";
                }

                if (Type == EffectType.EntryPortal)
                {
                    return $"Array(Vector({Coordinate}), {Radius}, 5, {Prime})";
                }

                if (Type == EffectType.ExitPortal)
                {
                    return $"Array(Vector({Coordinate}), {Radius}, 6, {Prime})";
                }

                else
                {
                    return "";
                }
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

        public int GeneratePrimeForCP()
        {
            int prime = 1;
            foreach (CheckBox c in cpAbilGroupBox.Controls.OfType<CheckBox>())
            {
                if (!c.Checked)
                {
                    prime *= int.Parse(c.Tag.ToString());
                }
            }
            return prime;
        }

        public int GeneratePrimeForEffect()
        {
            int prime = 1;
            foreach (CheckBox c in effAbilGroupBox.Controls.OfType<CheckBox>())
            {
                if (!c.Checked)
                {
                    prime *= int.Parse(c.Tag.ToString());
                }
            }
            return prime;
        }

        public MissionType GetSelectedMissionType()
        {
            return missTypeComboBox.SelectedIndex switch
            {
                0 => MissionType.NoRocketPunch,
                1 => MissionType.NoPowerblock,
                2 => MissionType.NoSlam,
                3 => MissionType.Stalless,
                4 => MissionType.Spin,
                5 => MissionType.RocketPunchFirst,
                6 => MissionType.PowerblockFirst,
                7 => MissionType.SlamFirst,
                8 => MissionType.UpwardsDiag,
                9 => MissionType.DownwardsDiag,
                10 => MissionType.PunchBounce
            };
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

            int checkcount = 0;
            foreach (CheckBox z in effAbilGroupBox.Controls.OfType<CheckBox>())
            {
                if (z.Checked)
                {
                    checkcount++;
                }
            }
            if (checkcount == 0)
            {
                isNoChange.Checked = true;
            }
        }

        public void SelectTheCPtype(CheckpointType type)
        {
            if (type == CheckpointType.LevelStart)
            {
                isLvlStartCP.Checked = true;
            }
            if (type == CheckpointType.LevelEnd)
            {
                isLvlEndCP.Checked = true;
            }
            if (type == CheckpointType.Normal)
            {
                isNormalCP.Checked = true;
            }
        }

        private void mapTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Text == "Map")
            {
                if (e.Node.FirstNode?.Text != "Checkpoint 0") //level selector cp hasn't been added
                {
                    ClearCPControls();
                    HideEffectsMissions();
                    cpSettingsGroupBox.Enabled = true;
                    cpSettingsGroupBox.Location = new Point(368, 26);
                    cpSettingsGroupBox.Visible = true;
                    addCpBtn.Text = "Add Checkpoint";
                    isTeleport.Enabled = false;
                    isLvlSelector.Enabled = true;
                    isLvlSelector.Checked = true;
                    isLvlStartCP.Enabled = false;
                    isEffLocked.Enabled = true;
                    lvlGroupBox.Visible = false;
                    isLvlEndCP.Enabled = false;
                    isNormalCP.Enabled = false;
                    return;
                }

                else //level selector cp exists and now can add a level
                {
                    cpSettingsGroupBox.Visible = false;
                    HideEffectsMissions();
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
                isEffLocked.Enabled = true;
                return;
            }

            if (e.Node.Text.Contains("Level -")) //a level is selected
            {
                ClearCPControls();
                EnableCPControls();
                isLvlSelector.Enabled = false;
                isNormalCP.Checked = true;
                string currentLvlName = mapTreeView.SelectedNode.Tag.ToString();
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
                    addLvlBtn.Text = "Change Level Name";
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
                    isEffLocked.Enabled = true;
                    isNormalCP.Enabled = true;
                }
                lvlGroupBox.Visible = true;
                cpSettingsGroupBox.Location = new Point(368, 94);
                cpSettingsGroupBox.Visible = true;
                HideEffectsMissions();
                addLvlBtn.Text = "Change Level Name";
                addLvlBtn.Enabled = true;
                addCpBtn.Text = "Add Checkpoint";
                addCpBtn.Enabled = true;
                lvlNameTxt.Text = e.Node.Tag.ToString();
                return;
            }


            if (e.Node.Text == "Effects") //effects tree selected
            {
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
                addMissBtn.Text = "Add Mission";
                missSettingsGroupBox.Visible = true;
                cpSettingsGroupBox.Visible = false;
                lvlGroupBox.Visible = false;
                effSettingsGroupBox.Visible = false;
                return;
            }

            if (e.Node.Text.Contains("Checkpoint")) //a checkpoint under a level selected
            {
                string currentLvlName = mapTreeView.SelectedNode.Parent.Tag.ToString();
                bool doesLvlFirstCpExist = map.Levels.First(x => x.Name == currentLvlName).Checkpoints.Any(x => x.Type == CheckpointType.LevelStart);
                bool doesLvlLastCpExist = map.Levels.First(x => x.Name == currentLvlName).Checkpoints.Any(x => x.Type == CheckpointType.LevelEnd);
                isLvlEndCP.Enabled = doesLvlLastCpExist ? false : true;
                isLvlStartCP.Enabled = doesLvlFirstCpExist ? false : true;
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
                isEffLocked.Enabled = true;
                isNormalCP.Enabled = true;
                isTeleport.Enabled = true;
                isTeleport.Checked = map.Levels.First(x => x.Name == currentLvlName).Checkpoints[e.Node.Index].TeleportCoordinate == "" ? false : true;
                tpCoordTxt.Text = map.Levels.First(x => x.Name == currentLvlName).Checkpoints[e.Node.Index].TeleportCoordinate;
                tpRadTxt.Value = decimal.Parse(map.Levels.First(x => x.Name == currentLvlName).Checkpoints[e.Node.Index].TeleportRadius);
                Debug.WriteLine($"Level: {currentLvlName} | Checkpoint: {e.Node.Index}");
                return;
            }

            if (e.Node.Text.Contains("Effect:")) //effect of a checkpoint selected
            {
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
                if (currenteff.Type == MissionType.NoRocketPunch)
                {
                    missTypeComboBox.SelectedIndex = 0;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                }

                if (currenteff.Type == MissionType.NoSlam)
                {
                    missTypeComboBox.SelectedIndex = 2;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                }

                if (currenteff.Type == MissionType.NoPowerblock)
                {
                    missTypeComboBox.SelectedIndex = 1;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                }

                if (currenteff.Type == MissionType.Stalless)
                {
                    missTypeComboBox.SelectedIndex = 3;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                }

                if (currenteff.Type == MissionType.Spin)
                {
                    missTypeComboBox.SelectedIndex = 4;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                }

                if (currenteff.Type == MissionType.RocketPunchFirst)
                {
                    missTypeComboBox.SelectedIndex = 5;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                }

                if (currenteff.Type == MissionType.PowerblockFirst)
                {
                    missTypeComboBox.SelectedIndex = 6;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                }

                if (currenteff.Type == MissionType.SlamFirst)
                {
                    missTypeComboBox.SelectedIndex = 7;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                }

                if (currenteff.Type == MissionType.UpwardsDiag)
                {
                    missTypeComboBox.SelectedIndex = 8;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                }

                if (currenteff.Type == MissionType.DownwardsDiag)
                {
                    missTypeComboBox.SelectedIndex = 9;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                }

                if (currenteff.Type == MissionType.PunchBounce)
                {
                    missTypeComboBox.SelectedIndex = 10;
                    isMissTime.Checked = currenteff.isTimeMission;
                    isMissLock.Checked = currenteff.isLock;
                    missTimeUpDown.Value = decimal.Parse(currenteff.TimeValue);
                }

                addMissBtn.Text = "Edit Mission";
                cpSettingsGroupBox.Visible = false;
                lvlGroupBox.Visible = false;
                missSettingsGroupBox.Visible = true;
                effSettingsGroupBox.Visible = false;
                return;
            }
        }

        private void isTeleport_CheckedChanged(object sender, EventArgs e)
        {
            tpCoordTxt.Enabled = isTeleport.Checked;
            tpRadTxt.Enabled = isTeleport.Checked;
        }

        public int GetLevelIndexFromSelectedLevelNode()
        {
            if (mapTreeView.Nodes[0].FirstNode.Text == "Checkpoint 0")
            {
                return mapTreeView.SelectedNode.Index - 1;
            }
            else
            {
                return mapTreeView.SelectedNode.Index;
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
                map.Levels.Add(newLvl);
                TreeNode lvlNode = new TreeNode("Level - " + lvlNameTxt.Text);
                lvlNode.Tag = lvlNameTxt.Text;
                mapTreeView.Nodes[0].Nodes.Add(lvlNode);
                mapTreeView.SelectedNode = mapTreeView.Nodes[0].LastNode;
                return;
            }

            if (addLvlBtn.Text == "Change Level Name")
            {
                if (string.IsNullOrWhiteSpace(lvlNameTxt.Text) || string.IsNullOrEmpty(lvlNameTxt.Text)) //lvlnameTxt was empty
                {
                    MessageBox.Show("Please enter a level name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (mapTreeView.SelectedNode.Text == lvlNameTxt.Text) //user put the same level name 
                {
                    MessageBox.Show("Please enter a different level name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (map.Levels.Any(x => x.Name == lvlNameTxt.Text)) //user put a level name that already exists
                {
                    MessageBox.Show("A level with this name already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                //map.Levels[mapTreeView.SelectedNode.Index].Name = lvlNameTxt.Text;
                Debug.WriteLine(GetLevelIndexFromSelectedLevelNode());
                map.Levels.First(x => x.Name == mapTreeView.SelectedNode.Tag.ToString()).Name = lvlNameTxt.Text;
                mapTreeView.SelectedNode.Tag = lvlNameTxt.Text;
                mapTreeView.SelectedNode.Text = "Level - " + lvlNameTxt.Text;
                lvlNameTxt.Text = "";
                return;
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
                    if (IsAbilCount())
                    {
                        newCP.isAbilCount = true;
                        newCP.PunchCount = punchUpDown.Value.ToString();
                        newCP.SlamCount = slamUpDown.Value.ToString();
                        newCP.PowerblockCount = powerBlockUpDown.Value.ToString();
                    }
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
                    if (IsAbilCount())
                    {
                        newCP.isAbilCount = true;
                        newCP.PunchCount = punchUpDown.Value.ToString();
                        newCP.SlamCount = slamUpDown.Value.ToString();
                        newCP.PowerblockCount = powerBlockUpDown.Value.ToString();
                    }
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
                    if (IsAbilCount())
                    {
                        newCP.isAbilCount = true;
                        newCP.PunchCount = punchUpDown.Value.ToString();
                        newCP.SlamCount = slamUpDown.Value.ToString();
                        newCP.PowerblockCount = powerBlockUpDown.Value.ToString();
                    }


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
                    if (IsAbilCount())
                    {
                        map.levelSelectorCP.isAbilCount = true;
                        map.levelSelectorCP.PunchCount = punchUpDown.Value.ToString();
                        map.levelSelectorCP.SlamCount = slamUpDown.Value.ToString();
                        map.levelSelectorCP.PowerblockCount = powerBlockUpDown.Value.ToString();
                    }
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
                    if (IsAbilCount())
                    {
                        map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].isAbilCount = true;
                        map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].PunchCount = punchUpDown.Value.ToString();
                        map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].SlamCount = slamUpDown.Value.ToString();
                        map.Levels.First(x => x.Name == currentLvlName).Checkpoints[cpindex].PowerblockCount = powerBlockUpDown.Value.ToString();
                    }
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
                    mapTreeView.SelectedNode.Remove();
                    mapTreeView.SelectedNode = mapTreeView.TopNode.Nodes[int.Parse(needed.Text)];
                    //change the name of the other checkpoints accordingly
                    for (int i = 1; i < mapTreeView.SelectedNode.Nodes.Count; i++)
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
                    mapTreeView.SelectedNode.Remove();
                    mapTreeView.SelectedNode = mapTreeView.SelectedNode.Parent;
                    for (int i = 1; i < mapTreeView.SelectedNode.Nodes.Count; i++)
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
                    for (int i = 1; i < mapTreeView.SelectedNode.Nodes.Count; i++)
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

                if (effTypeComboBox.SelectedItem.ToString() == "Permation")
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

                if (effTypeComboBox.SelectedItem.ToString() == "Permation")
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
            if (effTypeComboBox.SelectedItem.ToString() == "Time")
            {
                timePanel.Visible = true;
                effAbilGroupBox.Enabled = false;
                issEffCdReset.Enabled = false;
                iseffLightShaft.Enabled = true;
                return;
            }

            if (effTypeComboBox.SelectedItem.ToString() == "Death")
            {
                timePanel.Visible = false;
                effAbilGroupBox.Enabled = false;
                issEffCdReset.Enabled = false;
                iseffLightShaft.Enabled = true;
                return;
            }

            if (effTypeComboBox.SelectedItem.ToString() == "Ability")
            {
                timePanel.Visible = false;
                effAbilGroupBox.Enabled = true;
                issEffCdReset.Enabled = true;
                iseffLightShaft.Enabled = true;
            }

            if (effTypeComboBox.SelectedItem.ToString() == "Permeation")
            {
                timePanel.Visible = false;
                effAbilGroupBox.Enabled = true;
                issEffCdReset.Enabled = true;
                iseffLightShaft.Enabled = true;
            }

            if (effTypeComboBox.SelectedItem.ToString() == "Safepoint")
            {
                timePanel.Visible = false;
                effAbilGroupBox.Enabled = true;
                issEffCdReset.Enabled = false;
                iseffLightShaft.Enabled = false;
            }

            if (effTypeComboBox.SelectedItem.ToString() == "Entry Portal")
            {
                timePanel.Visible = false;
                effAbilGroupBox.Enabled = true;
                issEffCdReset.Enabled = true;
                iseffLightShaft.Enabled = false;
            }

            if (effTypeComboBox.SelectedItem.ToString() == "Exit Portal")
            {
                timePanel.Visible = false;
                effAbilGroupBox.Enabled = true;
                issEffCdReset.Enabled = true;
                iseffLightShaft.Enabled = false;
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

                if (missTypeComboBox.SelectedItem.ToString().Contains("First") && (pncfirst || pwbfirst || slmfirst))
                {
                    MessageBox.Show("You can only have one of 'X Ability First' missions per checkpoint!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

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
                bool pncfirst = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.RocketPunchFirst);
                bool pwbfirst = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.PowerblockFirst);
                bool slmfirst = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.SlamFirst);

                if (missTypeComboBox.SelectedItem.ToString().Contains("First") && (pncfirst || pwbfirst || slmfirst))
                {
                    MessageBox.Show("You can only have one of 'X Ability First' missions per checkpoint!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (missTypeComboBox.SelectedItem.ToString() == "No Rocket Punch")
                {
                    bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.NoRocketPunch);
                    if (exists)
                    {
                        MessageBox.Show("You've already added the 'No Rocket Punch' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions)[missindex] = new Mission
                    {
                        Type = MissionType.NoRocketPunch,
                        isLock = isMissLock.Checked,
                        isTimeMission = isMissTime.Checked,
                        TimeValue = missTimeUpDown.Value.ToString(),
                    };
                    mapTreeView.SelectedNode.Text = "Mission: No Rocket Punch";
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

                    (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions)[missindex] = new Mission
                    {
                        Type = MissionType.NoPowerblock,
                        isLock = isMissLock.Checked,
                        isTimeMission = isMissTime.Checked,
                        TimeValue = missTimeUpDown.Value.ToString()
                    };
                    mapTreeView.SelectedNode.Text = "Mission: No Powerblock";
                }

                if (missTypeComboBox.SelectedItem.ToString() == "No Seismic Slam")
                {
                    bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.NoSlam);
                    if (exists)
                    {
                        MessageBox.Show("You've already added the 'No Seismic Slam' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions)[missindex] = new Mission
                    {
                        Type = MissionType.NoSlam,
                        isLock = isMissLock.Checked,
                        isTimeMission = isMissTime.Checked,
                        TimeValue = missTimeUpDown.Value.ToString()
                    };
                    mapTreeView.SelectedNode.Text = "Mission: No Seismic Slam";
                }

                if (missTypeComboBox.SelectedItem.ToString() == "Stalless")
                {
                    bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.Stalless);
                    if (exists)
                    {
                        MessageBox.Show("You've already added the 'Stalless' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions)[missindex] = new Mission
                    {
                        Type = MissionType.Stalless,
                        isLock = isMissLock.Checked,
                        isTimeMission = isMissTime.Checked,
                        TimeValue = missTimeUpDown.Value.ToString()
                    };
                    mapTreeView.SelectedNode.Text = "Mission: Stalless";
                }

                if (missTypeComboBox.SelectedItem.ToString() == "360 Spin")
                {
                    bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.Spin);
                    if (exists)
                    {
                        MessageBox.Show("You've already added the '360' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions)[missindex] = new Mission
                    {
                        Type = MissionType.Spin,
                        isLock = isMissLock.Checked,
                        isTimeMission = isMissTime.Checked,
                        TimeValue = missTimeUpDown.Value.ToString()
                    };
                    mapTreeView.SelectedNode.Text = "Mission: 360 Spin";
                }

                if (missTypeComboBox.SelectedItem.ToString() == "Use Rocket Punch First")
                {
                    bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.RocketPunchFirst);
                    if (exists)
                    {
                        MessageBox.Show("You've already added the 'Use Rocket Punch First' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions)[missindex] = new Mission
                    {
                        Type = MissionType.RocketPunchFirst,
                        isLock = isMissLock.Checked,
                        isTimeMission = isMissTime.Checked,
                        TimeValue = missTimeUpDown.Value.ToString()
                    };
                    mapTreeView.SelectedNode.Text = "Mission: Use Rocket Punch First";
                }

                if (missTypeComboBox.SelectedItem.ToString() == "Use Powerblock First")
                {
                    bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.PowerblockFirst);
                    if (exists)
                    {
                        MessageBox.Show("You've already added the 'Use Powerblock First' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions)[missindex] = new Mission
                    {
                        Type = MissionType.PowerblockFirst,
                        isLock = isMissLock.Checked,
                        isTimeMission = isMissTime.Checked,
                        TimeValue = missTimeUpDown.Value.ToString()
                    };
                    mapTreeView.SelectedNode.Text = "Mission: Use Powerblock First";
                }

                if (missTypeComboBox.SelectedItem.ToString() == "Use Seismic Slam First")
                {
                    bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.SlamFirst);
                    if (exists)
                    {
                        MessageBox.Show("You've already added the 'Use Seismic Slam First' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions)[missindex] = new Mission
                    {
                        Type = MissionType.SlamFirst,
                        isLock = isMissLock.Checked,
                        isTimeMission = isMissTime.Checked,
                        TimeValue = missTimeUpDown.Value.ToString()
                    };
                    mapTreeView.SelectedNode.Text = "Mission: Use Seismic Slam First";
                }

                if (missTypeComboBox.SelectedItem.ToString() == "Upwards Diagonal Punch")
                {
                    bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.UpwardsDiag);
                    if (exists)
                    {
                        MessageBox.Show("You've already added the 'Upwards Diagonal Punch' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions)[missindex] = new Mission
                    {
                        Type = MissionType.UpwardsDiag,
                        isLock = isMissLock.Checked,
                        isTimeMission = isMissTime.Checked,
                        TimeValue = missTimeUpDown.Value.ToString()
                    };
                    mapTreeView.SelectedNode.Text = "Mission: Upwards Diagonal Punch";
                }

                if (missTypeComboBox.SelectedItem.ToString() == "Downwards Diagonal Punch")
                {
                    bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.DownwardsDiag);
                    if (exists)
                    {
                        MessageBox.Show("You've already added the 'Downwards Diagonal Punch' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions)[missindex] = new Mission
                    {
                        Type = MissionType.DownwardsDiag,
                        isLock = isMissLock.Checked,
                        isTimeMission = isMissTime.Checked,
                        TimeValue = missTimeUpDown.Value.ToString()
                    };
                    mapTreeView.SelectedNode.Text = "Mission: Downwards Diagonal Punch";
                }

                if (missTypeComboBox.SelectedItem.ToString() == "Rocket Punch Bounce")
                {
                    bool exists = (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions).Any(x => x.Type == MissionType.PunchBounce);
                    if (exists)
                    {
                        MessageBox.Show("You've already added the 'Rocket Punch Bounce' mission!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    (mapcp ? map.levelSelectorCP.Missions : map.Levels[lvlindex].Checkpoints[cpindex].Missions)[missindex] = new Mission
                    {
                        Type = MissionType.PunchBounce,
                        isLock = isMissLock.Checked,
                        isTimeMission = isMissTime.Checked,
                        TimeValue = missTimeUpDown.Value.ToString()
                    };
                    mapTreeView.SelectedNode.Text = "Mission: Rocket Punch Bounce";
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

        private void clipboardLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Clipboard.SetText(map.GenerateMapData());
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
                    treeMenuStrip.Items[0].Text = "Delete Level";
                }

                if (e.Node.Text.Contains("Checkpoint"))
                {
                    treeMenuStrip.Items[0].Text = "Delete Checkpoint";
                }

                if (e.Node.Text.Contains("Effect:"))
                {
                    treeMenuStrip.Items[0].Text = "Delete Effect";
                }

                if (e.Node.Text.Contains("Mission:"))
                {
                    treeMenuStrip.Items[0].Text = "Delete Mission";
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
                    mapTreeView.SelectedNode.Remove();
                    map.Levels.Find(x => x.Name == lvlname).Checkpoints.RemoveAt(cpindex);
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

        private void aboutLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("Made for making map data generation easier in the absence of inspector. " +
                "Big thanks to dreadowl, this would be IMPOSSIBLE without his help.- Dorian", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    public static class EffectExtensions
    {
        public static string ToNodeString(this Effect me)
        {
            if (me.Type == EffectType.Time)
            {
                return "Effect: Time " + (me.Lightshaft ? "Lightshaft - " : "Orb - ") + me.TimeValue;
            }

            if (me.Type == EffectType.Death)
            {
                return "Effect: Death " + (me.Lightshaft ? "Lightshaft" : "Orb");
            }

            if (me.Type == EffectType.Ability)
            {
                return "Effect: Ability " + (me.Lightshaft ? "Lightshaft" : "Orb");
            }

            if (me.Type == EffectType.Permeation)
            {
                return "Effect: Permeation " + (me.Lightshaft ? "Lightshaft" : "Orb");
            }

            if (me.Type == EffectType.Safepoint)
            {
                return "Effect: Safepoint";
            }

            if (me.Type == EffectType.EntryPortal)
            {
                return "Effect: Entry Portal";
            }

            if (me.Type == EffectType.ExitPortal)
            {
                return "Effect: Exit Portal";
            }

            return "";
        }
    }

    public static class MissionExtensions
    {
        public static string ToNodeString(this Mission me)
        {
            if (me.Type == MissionType.NoRocketPunch)
            {
                return "Mission: No Rocket Punch";
            }

            if (me.Type == MissionType.NoSlam)
            {
                return "Mission: No Seismic Slam";
            }

            if (me.Type == MissionType.NoPowerblock)
            {
                return "Mission: No Powerblock";
            }

            if (me.Type == MissionType.Stalless)
            {
                return "Mission: Stalless";
            }

            if (me.Type == MissionType.Spin)
            {
                return "Mission: 360 Spin";
            }
            
            if (me.Type == MissionType.RocketPunchFirst)
            {
                return "Mission: Rocket Punch First";
            }

            if (me.Type == MissionType.PowerblockFirst)
            {
                return "Mission: Powerblock First";
            }

            if (me.Type == MissionType.SlamFirst)
            {
                return "Mission: Seismic Slam First";
            }

            if (me.Type == MissionType.PunchBounce)
            {
                return "Mission: Rocket Punch Bounce";
            }

            return "";
        }
    }
}