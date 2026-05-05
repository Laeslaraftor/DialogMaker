using DialogMaker.Core.Common;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Core.Executioning.Internal;
using MessagePack;
using Newtonsoft.Json;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectEmotion : DialogProjectResourceObject, IEmotion
    {
        public DialogProjectEmotion(DialogProjectResources resources)
            : base(resources, Guid.NewGuid())
        {
            LeftEye = new();
            RightEye = new();
            Mouth = new();
        }
        public DialogProjectEmotion(DialogProjectResources resources, DialogProjectEmotionSavedState savedState)
            : base(resources, savedState)
        {
            LeftEye = savedState.LeftEye ?? new();
            RightEye = savedState.RightEye ?? new();
            Mouth = savedState.Mouth ?? new();
        }

        public override DialogResourceType ResourceType => DialogResourceType.Emotion;
        public EyeInfo LeftEye { get; }
        public EyeInfo RightEye { get; }
        public MouthInfo Mouth { get; }

        IEmotion.IEye IEmotion.LeftEye => LeftEye;
        IEmotion.IEye IEmotion.RightEye => RightEye;
        IEmotion.IMouth IEmotion.Mouth => Mouth;

        #region Управление

        public override IVariable ToVariable()
        {
            return new LocalVariable(Id);
        }

        protected override DialogProjectResourceObjectSavedState CreateSavedState()
        {
            return new DialogProjectEmotionSavedState()
            {
                LeftEye = LeftEye,
                RightEye = RightEye,
                Mouth = Mouth,
            };
        }

        #endregion

        #region Классы

        [MessagePackObject(SuppressSourceGeneration = false)]
        public class EyebrowInfo() : ObservableObject, IEmotion.IEyebrow
        {
            public EyebrowInfo(IEmotion.IEyebrow eyebrow)
                : this()
            {
                YPosition = eyebrow.YPosition;
                ZRotation = eyebrow.ZRotation;
            }

            [Key(0), JsonProperty("yPosition")]
            [Name("Положение по вертикали"), Range(-1, 1)]
            public float YPosition
            {
                get => field;
                set
                {
                    if (field != value)
                    {
                        value = Helper.Clamp(value, -1, 1);
                        OnPropertyChanging(nameof(YPosition));
                        field = value;
                        OnPropertyChanged(nameof(YPosition));
                    }
                }
            }
            [Key(1), JsonProperty("zRotation")]
            [Name("Наклон"), Range(-1, 1)]
            public float ZRotation
            {
                get => field;
                set
                {
                    if (field != value)
                    {
                        value = Helper.Clamp(value, -1, 1);
                        OnPropertyChanging(nameof(ZRotation));
                        field = value;
                        OnPropertyChanged(nameof(ZRotation));
                    }
                }
            }
        }
        [MessagePackObject(SuppressSourceGeneration = false)]
        public class EyeInfo() : ObservableObject, IEmotion.IEye
        {
            public EyeInfo(IEmotion.IEye eye)
                : this()
            {
                Eyebrow = new(eye.Eyebrow);
                ClosePercent = eye.ClosePercent;
            }

            [Key(1), JsonProperty("eyebrow")]
            public EyebrowInfo Eyebrow
            {
                get => field;
                set
                {
                    if (field != value)
                    {
                        OnPropertyChanging(nameof(Eyebrow));
                        field = value;
                        OnPropertyChanged(nameof(Eyebrow));
                    }
                }
            } = new();
            [Key(2), JsonProperty("closePercent")]
            [Name("Степень закрытости"), Range(0, 1)]
            public float ClosePercent
            {
                get => field;
                set
                {
                    if (field != value)
                    {
                        value = Helper.Clamp01(value);
                        OnPropertyChanging(nameof(ClosePercent));
                        field = value;
                        OnPropertyChanged(nameof(ClosePercent));
                    }
                }
            }

            [JsonIgnore]
            IEmotion.IEyebrow IEmotion.IEye.Eyebrow => Eyebrow;
        }
        [MessagePackObject(SuppressSourceGeneration = false)]
        public class MouthInfo() : ObservableObject, IEmotion.IMouth
        {
            public MouthInfo(IEmotion.IMouth mouth)
                : this()
            {
                OpenPercent = mouth.OpenPercent;
                HorizontalStretchPercent = mouth.HorizontalStretchPercent;
                LeftCornerYPosition = mouth.LeftCornerYPosition;
                RightCornerYPosition = mouth.RightCornerYPosition;
            }

            [Key(0), JsonProperty("openPercent")]
            [Name("Степень открытости"), Range(0, 1)]
            public float OpenPercent
            {
                get => field;
                set
                {
                    if (field != value)
                    {
                        value = Helper.Clamp01(value);
                        OnPropertyChanging(nameof(OpenPercent));
                        field = value;
                        OnPropertyChanged(nameof(OpenPercent));
                    }
                }
            }
            [Key(1), JsonProperty("horizontalStretchPercent")]
            [Name("Сжатие по горизонтали"), Range(0, 1)]
            public float HorizontalStretchPercent
            {
                get => field;
                set
                {
                    if (field != value)
                    {
                        value = Helper.Clamp01(value);
                        OnPropertyChanging(nameof(HorizontalStretchPercent));
                        field = value;
                        OnPropertyChanged(nameof(HorizontalStretchPercent));
                    }
                }
            }
            [Key(2), JsonProperty("leftCornerYPosition")]
            [Name("Смещение левого уголка рта"), Range(-1, 1)]
            public float LeftCornerYPosition
            {
                get => field;
                set
                {
                    if (field != value)
                    {
                        value = Helper.Clamp(value, -1, 1);
                        OnPropertyChanging(nameof(LeftCornerYPosition));
                        field = value;
                        OnPropertyChanged(nameof(LeftCornerYPosition));
                    }
                }
            }
            [Key(3), JsonProperty("rightCornerYPosition")]
            [Name("Смещение правого уголка рта"), Range(-1, 1)]
            public float RightCornerYPosition
            {
                get => field;
                set
                {
                    if (field != value)
                    {
                        value = Helper.Clamp(value, -1, 1);
                        OnPropertyChanging(nameof(RightCornerYPosition));
                        field = value;
                        OnPropertyChanged(nameof(RightCornerYPosition));
                    }
                }
            }
        }

        #endregion
    }
}
