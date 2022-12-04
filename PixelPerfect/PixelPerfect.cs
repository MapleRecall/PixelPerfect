using System;
using System.Diagnostics;
using Dalamud.Configuration;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.Plugin;
using ImGuiNET;
using Num = System.Numerics;

namespace PixelPerfect
{
    public class PixelPerfect : IDalamudPlugin
    {
        public string Name => "Pixel Perfect";
        private readonly DalamudPluginInterface _pi;
        private readonly CommandManager _cm;
        private readonly ClientState _cs;
        private readonly Framework _fw;
        private readonly GameGui _gui;
        private readonly Condition _condition;
        
        private readonly Config _configuration;
        private bool _enabled;
        private bool _config;
        private bool _combat;
        private bool _circle;
        private bool _instance;
        private bool _cutscene;
        private Num.Vector4 _col = new Num.Vector4(1f, 1f, 1f, 1f);
        private Num.Vector4 _col2 = new Num.Vector4(0.4f, 0.4f, 0.4f, 1f);
        //ring
        private bool _ring;
        private Num.Vector4 _colRing = new Num.Vector4(0.4f, 0.4f, 0.4f, 0.5f);
        private float _radius = 10f;
        private int _segments = 100;
        private float _thickness = 10f;
        //ring2
        private bool _ring2;
        private Num.Vector4 _colRing2 = new Num.Vector4(0.4f, 0.4f, 0.4f, 0.5f);
        private float _radius2 = 10f;
        private int _segments2 = 100;
        private float _thickness2 = 10f;
        //north stuff
        private bool _north1;
        private bool _north2;
        private bool _north3;
        private float _lineOffset = 0.6f;
        private float _lineLength = 1f;
        private float _chevLength = 0.5f;
        private float _chevOffset = 0.5f;
        private float _chevRad = 0.5f;
        private float _chevSin = 0.5f;
        private float _chevThicc = 10f;
        private float _lineThicc = 10f;
        private Num.Vector4 _chevCol = new Num.Vector4(1f, 1f, 1f, 1f);
        private Num.Vector4 _lineCol = new Num.Vector4(1f, 1f, 1f, 1f);
        private int dirtyHack = 0;
        

        public PixelPerfect(
            DalamudPluginInterface pluginInterface,
            CommandManager commandManager,
            ClientState clientState,
            Framework framework,
            GameGui gameGui,
            Condition condition
        )
        {
            _pi = pluginInterface;
            _cm = commandManager;
            _cs = clientState;
            _fw = framework;
            _gui = gameGui;
            _condition = condition;

            _configuration = pluginInterface.GetPluginConfig() as Config ?? new Config();
            _ring = _configuration.Ring;
            _thickness = _configuration.Thickness;
            _colRing = _configuration.ColRing;
            _segments = _configuration.Segments;
            _radius = _configuration.Radius;
            _enabled = _configuration.Enabled;
            _combat = _configuration.Combat;
            _circle = _configuration.Circle;
            _instance = _configuration.Instance;
            _cutscene = _configuration.Cutscene;
            _col = _configuration.Col;
            _col2 = _configuration.Col2;
            _ring2 = _configuration.Ring2;
            _thickness2 = _configuration.Thickness2;
            _colRing2 = _configuration.ColRing2;
            _segments2 = _configuration.Segments2;
            _radius2 = _configuration.Radius2;
            _north1 = _configuration.North1;
            _north2 = _configuration.North2;
            _north3 = _configuration.North3;
            _lineOffset = _configuration.LineOffset;
            _lineLength = _configuration.LineLength;
            _chevLength = _configuration.ChevLength;
            _chevOffset = _configuration.ChevOffset;
            _chevRad = _configuration.ChevRad;
            _chevSin = _configuration.ChevSin;
            _chevThicc = _configuration.ChevThicc;
            _lineThicc = _configuration.LineThicc;
            _chevCol = _configuration.ChevCol;
            _lineCol = _configuration.LineCol;

            pluginInterface.UiBuilder.Draw += DrawWindow;
            pluginInterface.UiBuilder.OpenConfigUi += ConfigWindow;
            commandManager.AddHandler("/pp", new CommandInfo(Command)
            {
                HelpMessage = "Pixel Perfect 配置." +
                              "\n通过这些参数 'ring', 'ring2', 'north' 来开关这些功能."
            });
        }

        private void ConfigWindow()
        {
            _config = true;
        }


        public void Dispose()
        {
            _pi.UiBuilder.Draw -= DrawWindow;
            _pi.UiBuilder.OpenConfigUi -= ConfigWindow;
            _cm.RemoveHandler("/pp");
        }


        private void DrawWindow()
        {
            if (_config)
            {
                ImGui.SetNextWindowSize(new Num.Vector2(300, 500), ImGuiCond.FirstUseEver);
                ImGui.Begin("Pixel Perfect 配置", ref _config);

                ImGui.Checkbox("只在战斗启用", ref _combat);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("战斗外不显示");
                }
                ImGui.SameLine();
                ImGui.Checkbox("只在副本启用", ref _instance);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("例如迷宫、讨伐");
                }
                ImGui.SameLine();
                ImGui.Checkbox("过场动画中启用", ref _cutscene);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("比如战斗中的转场与黑屏");
                }

                ImGui.Separator();
                ImGui.Checkbox("判定点", ref _enabled);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("一个方便擦弹的判定点");
                }
                if (_enabled)
                {
                    ImGui.SameLine();
                    ImGui.ColorEdit4("判定点颜色", ref _col, ImGuiColorEditFlags.NoInputs);
                }
                ImGui.Checkbox("判定点描边", ref _circle);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("一个围绕判定点的小小圆环");
                }
                if (_circle)
                {
                    ImGui.SameLine();
                    ImGui.ColorEdit4("描边颜色", ref _col2, ImGuiColorEditFlags.NoInputs);
                }
                ImGui.Checkbox("显示指北", ref _north1);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("显示一个始终指向正北边的标记来避免迷失人生的方向");
                }
                if (_north1)
                {
                    ImGui.SameLine();
                    ImGui.Checkbox("线", ref _north2);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("这是一条线…");
                    }
                    ImGui.SameLine();
                    ImGui.Checkbox("鏃", ref _north3);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("“箭头”的高级称呼。");
                    }
                    if (_north2)
                    {
                        ImGui.DragFloat("线偏移", ref _lineOffset);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("线的起始点与人物判定点的距离");
                        }
                        ImGui.DragFloat("线长", ref _lineLength);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("线的长度\n真的有必要给所有条目都加悬浮提示么？");
                        }
                        ImGui.DragFloat("线粗", ref _lineThicc);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("线的粗细\n英文可以变变语序，可这标签和提示翻译成中文完全一样啊！");
                        }
                        ImGui.ColorEdit4("线色", ref _lineCol, ImGuiColorEditFlags.NoInputs);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("线的颜色\n这个“色”，应该读“se”还是“shai”呢？");
                        }
                    }
                    if (_north3)
                    {
                        ImGui.DragFloat("鏃之始", ref _chevOffset);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("數學之由，皆有所不定。 亂用之。");
                        }
                        ImGui.DragFloat("鏃之長", ref _chevLength);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("數學之由，皆有所不定。 亂用之。");
                        }
                        ImGui.DragFloat("鏃之徑", ref _chevRad);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("數學之由，皆有所不定。 亂用之。");
                        }
                        ImGui.DragFloat("鏃之弦", ref _chevSin);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("數學之由，皆有所不定。 亂用之。");
                        }
                        ImGui.DragFloat("鏃之細", ref _chevThicc);
                        ImGui.ColorEdit4("鏃之色", ref _chevCol, ImGuiColorEditFlags.NoInputs);
                    }
                }




                ImGui.Separator();
                ImGui.Checkbox("圆环", ref _ring);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("在人物周围显示一个圆环。\n可用于标识技能的最大距离。");
                }
                if (_ring)
                {
                    ImGui.DragFloat("半径", ref _radius);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("单位为游戏内的[米]");
                    }
                    ImGui.DragFloat("粗细", ref _thickness);
                    ImGui.DragInt("平滑度", ref _segments);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("用于模拟圆形的正多边形的细分级数N\n当N趋近于无穷时将可以获得一个完美的圆形\n在拓扑学上，简单多边形和圆形同胚(Homeomorphism)");
                    }
                    ImGui.ColorEdit4("圆环颜色", ref _colRing, ImGuiColorEditFlags.NoInputs);
                }
                ImGui.Separator();
                ImGui.Checkbox("圆环 2", ref _ring2);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("在人物周围显示另一个圆环。\n可用于标识另一个技能的最大距离。");
                }
                if (_ring2)
                {
                    ImGui.DragFloat("半径 2", ref _radius2);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("单位为游戏内的[米]");
                    }
                    ImGui.DragFloat("粗细 2", ref _thickness2);
                    ImGui.DragInt("平滑度 2", ref _segments2);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("用于模拟圆形的正多边形的细分级数N\n当N趋近于无穷时将可以获得一个完美的圆形\n在拓扑学上，简单多边形和圆形同胚(Homeomorphism)");
                    }
                    ImGui.ColorEdit4("圆环颜色 2", ref _colRing2, ImGuiColorEditFlags.NoInputs);
                }



                if (ImGui.Button("保存 and Close 配置"))
                {
                    SaveConfig();
                    _config = false;
                }


                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);

                if (ImGui.Button("给 Haplo 买一杯热巧克力"))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://ko-fi.com/haplo",
                        UseShellExecute = true
                    });
                }

                ImGui.PopStyleColor(3);
                ImGui.End();

                if (dirtyHack > 100)
                {
                    SaveConfig();
                    dirtyHack = 0;
                }

                dirtyHack++;
            }

            if (_cs.LocalPlayer == null) return;

            if (_combat)
            {
                if (!_condition[ConditionFlag.InCombat])
                {
                    return;
                }
            }

            if (_instance)
            {
                if (!_condition[ConditionFlag.BoundByDuty])
                {
                    return;
                }

            }

            if (!_cutscene)
            {
                var cutsceneActive = _condition[ConditionFlag.OccupiedInCutSceneEvent] ||
                                     _condition[ConditionFlag.WatchingCutscene] ||
                                     _condition[ConditionFlag.WatchingCutscene78];
                if (cutsceneActive)
                {
                    return;
                }
            }

            var actor = _cs.LocalPlayer;
            if (!_gui.WorldToScreen(
                new Num.Vector3(actor.Position.X, actor.Position.Y, actor.Position.Z),
                out var pos)) return;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Num.Vector2(0, 0));
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Num.Vector2(0, 0));
            ImGui.Begin("Ring",
                ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground);
            ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);

            if (_enabled)
            {
                ImGui.GetWindowDrawList().AddCircleFilled(
                    new Num.Vector2(pos.X, pos.Y),
                    2f,
                    ImGui.GetColorU32(_col),
                    100);
            }
            if (_circle)
            {
                ImGui.GetWindowDrawList().AddCircle(
                    new Num.Vector2(pos.X, pos.Y),
                    2.2f,
                    ImGui.GetColorU32(_col2),
                    100);
            }

            if (_ring)
            {
                DrawRingWorld(_cs.LocalPlayer, _radius, _segments, _thickness,
                    ImGui.GetColorU32(_colRing));
            }
            if (_ring2)
            {
                DrawRingWorld(_cs.LocalPlayer, _radius2, _segments2, _thickness2,
                    ImGui.GetColorU32(_colRing2));
            }

            if (_north1)
            {
                //Tip of arrow
                _gui.WorldToScreen(new Num.Vector3(
                            actor.Position.X + ((_lineLength + _lineOffset) * (float)Math.Sin(Math.PI)),
                            actor.Position.Y,
                            actor.Position.Z + ((_lineLength + _lineOffset) * (float)Math.Cos(Math.PI))
                        ),
                        out Num.Vector2 lineTip);
                //Player + offset
                _gui.WorldToScreen(new Num.Vector3(
                        actor.Position.X + (_lineOffset * (float)Math.Sin(Math.PI)),
                        actor.Position.Y,
                        actor.Position.Z + (_lineOffset * (float)Math.Cos(Math.PI))
                    ),
                    out Num.Vector2 lineOffset);
                //Chev offset1
                _gui.WorldToScreen(new Num.Vector3(
                        actor.Position.X + (_chevOffset * (float)Math.Sin(Math.PI / _chevRad) * _chevSin),
                        actor.Position.Y,
                        actor.Position.Z + (_chevOffset * (float)Math.Cos(Math.PI / _chevRad) * _chevSin)
                    ),
                    out Num.Vector2 chevOffset1);
                //Chev offset2
                _gui.WorldToScreen(new Num.Vector3(
                        actor.Position.X + (_chevOffset * (float)Math.Sin(Math.PI / -_chevRad) * _chevSin),
                        actor.Position.Y,
                        actor.Position.Z + (_chevOffset * (float)Math.Cos(Math.PI / -_chevRad) * _chevSin)
                    ),
                    out Num.Vector2 chevOffset2);
                //Chev Tip
                _gui.WorldToScreen(new Num.Vector3(
                        actor.Position.X + ((_chevOffset + _chevLength) * (float)Math.Sin(Math.PI)),
                        actor.Position.Y,
                        actor.Position.Z + ((_chevOffset + _chevLength) * (float)Math.Cos(Math.PI))
                    ),
                    out Num.Vector2 chevTip);
                if (_north2)
                {
                    ImGui.GetWindowDrawList().AddLine(new Num.Vector2(lineTip.X, lineTip.Y), new Num.Vector2(lineOffset.X, lineOffset.Y),
                        ImGui.GetColorU32(_lineCol), _lineThicc);
                }
                if (_north3)
                {
                    ImGui.GetWindowDrawList().AddLine(new Num.Vector2(chevTip.X, chevTip.Y), new Num.Vector2(chevOffset1.X, chevOffset1.Y),
                        ImGui.GetColorU32(_chevCol), _chevThicc);
                    ImGui.GetWindowDrawList().AddLine(new Num.Vector2(chevTip.X, chevTip.Y), new Num.Vector2(chevOffset2.X, chevOffset2.Y),
                        ImGui.GetColorU32(_chevCol), _chevThicc);
                }
            }

            ImGui.End();
            ImGui.PopStyleVar();
        }


        private void Command(string command, string arguments)
        {
            if (arguments == "ring")
            {
                _ring = !_ring;
            }
            else if (arguments == "ring2")
            {
                _ring2 = !_ring2;
            }
            else if (arguments == "north")
            {
                _north1 = !_north1;
            }
            else
            {
                _config = !_config;
            }
            SaveConfig();
        }

        private void SaveConfig()
        {
            _configuration.Enabled = _enabled;
            _configuration.Combat = _combat;
            _configuration.Circle = _circle;
            _configuration.Instance = _instance; 
            _configuration.Cutscene = _cutscene;
            _configuration.Col = _col;
            _configuration.Col2 = _col2;
            _configuration.ColRing = _colRing;
            _configuration.Thickness = _thickness;
            _configuration.Segments = _segments;
            _configuration.Ring = _ring;
            _configuration.Radius = _radius;
            _configuration.Ring2 = _ring2;
            _configuration.Thickness2 = _thickness2;
            _configuration.ColRing2 = _colRing2;
            _configuration.Segments2 = _segments2;
            _configuration.Radius2 = _radius2;
            _configuration.North1 = _north1;
            _configuration.North2 = _north2;
            _configuration.North3 = _north3;
            _configuration.LineOffset = _lineOffset;
            _configuration.LineLength = _lineLength;
            _configuration.ChevLength = _chevLength;
            _configuration.ChevOffset = _chevOffset;
            _configuration.ChevRad = _chevRad;
            _configuration.ChevSin = _chevSin;
            _configuration.ChevThicc = _chevThicc;
            _configuration.LineThicc = _lineThicc;
            _configuration.ChevCol = _chevCol;
            _configuration.LineCol = _lineCol;
            _pi.SavePluginConfig(_configuration);
        }

        private void DrawRingWorld(Dalamud.Game.ClientState.Objects.Types.Character actor, float radius, int numSegments, float thicc, uint colour)
        {
            var seg = numSegments / 2;
            for (var i = 0; i <= numSegments; i++)
            {
                _gui.WorldToScreen(new Num.Vector3(
                    actor.Position.X + (radius * (float)Math.Sin((Math.PI / seg) * i)),
                    actor.Position.Y,
                    actor.Position.Z + (radius * (float)Math.Cos((Math.PI / seg) * i))
                    ),
                    out Num.Vector2 pos);
                ImGui.GetWindowDrawList().PathLineTo(new Num.Vector2(pos.X, pos.Y));
            }
            ImGui.GetWindowDrawList().PathStroke(colour, ImDrawFlags.None, thicc);
        }
    }


    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool Enabled { get; set; } = true;
        public bool Combat { get; set; } = true;
        public bool Circle { get; set; }
        public bool Instance { get; set; }
        public bool Cutscene { get; set; }
        public Num.Vector4 Col { get; set; } = new Num.Vector4(1f, 1f, 1f, 1f);
        public Num.Vector4 Col2 { get; set; } = new Num.Vector4(0.4f, 0.4f, 0.4f, 1f);
        public Num.Vector4 ColRing { get; set; } = new Num.Vector4(0.4f, 0.4f, 0.4f, 0.5f);
        public int Segments { get; set; } = 100;
        public float Thickness { get; set; } = 10f;
        public bool Ring { get; set; }
        public float Radius { get; set; } = 2f;
        public bool Ring2 { get; set; }
        public float Radius2 { get; set; } = 2f;
        public Num.Vector4 ColRing2 { get; set; } = new Num.Vector4(0.4f, 0.4f, 0.4f, 0.5f);
        public int Segments2 { get; set; } = 100;
        public float Thickness2 { get; set; } = 10f;
        public bool North1 { get; set; } = false;
        public bool North2 { get; set; } = false;
        public bool North3 { get; set; } = false;
        public float LineOffset { get; set; } = 0.5f;
        public float LineLength { get; set; } = 1f;
        public float ChevLength { get; set; } = 1f;
        public float ChevOffset { get; set; } = 1f;
        public float ChevRad { get; set; } = 11.5f;
        public float ChevSin { get; set; } = -1.5f;
        public float ChevThicc { get; set; } = 5f;
        public float LineThicc { get; set; } = 5f;
        public Num.Vector4 ChevCol { get; set; } = new Num.Vector4(0.4f, 0.4f, 0.4f, 0.5f);
        public Num.Vector4 LineCol { get; set; } = new Num.Vector4(0.4f, 0.4f, 0.4f, 0.5f);
    }
}