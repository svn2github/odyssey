using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;

namespace Odyssey.Controls
{
    //UNDONE: Section.Content sometimes not visible when IsMaximized has changed. 
    [TemplatePart(Name = partMinimizedButtonContainer)]
    public class OutlookBar : HeaderedItemsControl
    {
        const string partMinimizedButtonContainer = "PART_MinimizedContainer";
        static OutlookBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OutlookBar), new FrameworkPropertyMetadata(typeof(OutlookBar)));
        }

        private Collection<OutlookSection> maximizedSections;
        private Collection<OutlookSection> minimizedSections;
        private FrameworkElement minimizedButtonContainer;

        public OutlookBar()
            : base()
        {
            overflowMenu = new Collection<object>();
            SetValue(OutlookBar.OverflowMenuItemsPropertyKey, overflowMenu);
            SetValue(OutlookBar.OptionButtonsPropertyKey, new Collection<ButtonBase>());

            CommandBindings.Add(new CommandBinding(CollapseCommand, CollapseCommandExecuted));
            CommandBindings.Add(new CommandBinding(StartDraggingCommand, StartDraggingCommandExecuted));
            CommandBindings.Add(new CommandBinding(ShowPopupCommand, ShowPopupCommandExecuted));
            CommandBindings.Add(new CommandBinding(ResizeCommand, ResizeCommandExecuted));
            CommandBindings.Add(new CommandBinding(CloseCommand, CloseCommandExecuted));

            maximizedSections = new Collection<OutlookSection>();
            minimizedSections = new Collection<OutlookSection>();
            sections = new ObservableCollection<OutlookSection>();
            sections.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(SectionsCollectionChanged);
            this.SizeChanged += new SizeChangedEventHandler(OutlookBar_SizeChanged);
        }

        void OutlookBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplySections();
        }    



        private void CollapseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            IsMaximized ^= true;
        }

        private void ShowPopupCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!IsMaximized)
            {
                IsPopupVisible = true;
            }
        }

        private void ResizeCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Control c = e.OriginalSource as Control;
            if (c != null)
            {
                c.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(DragMouseLeftButtonUp);
            }
            this.PreviewMouseMove += new MouseEventHandler(PreviewMouseMoveResize);
        }

        private void CloseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        void PreviewMouseMoveResize(object sender, MouseEventArgs e)
        {
            Control c = e.OriginalSource as Control;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Dock == HorizontalAlignment.Left)
                {
                    ResizeFromRight(e);
                }
                else
                {
                    ResizeFromLeft(e);
                }
            }
            else this.PreviewMouseMove -= PreviewMouseMoveResize;
        }

        private void ResizeFromLeft(MouseEventArgs e)
        {
            Point pos = e.GetPosition(this);
            double w = this.ActualWidth - pos.X;

            if (w < 80)
            {
                w = double.NaN;
                IsMaximized = false;
            }
            else
            {
                IsMaximized = true;
            }
            if (MaxWidth != double.NaN && w > MaxWidth) w = MaxWidth;
            Width = w;
        }
        private void ResizeFromRight(MouseEventArgs e)
        {
            Point pos = e.GetPosition(this);
            double w = pos.X;

            if (w < 80)
            {
                w = double.NaN;
                IsMaximized = false;
            }
            else
            {
                IsMaximized = true;
            }
            if (MaxWidth != double.NaN && w > MaxWidth) w = MaxWidth;
            Width = w;
        }

        private void StartDraggingCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Control c = e.OriginalSource as Control;
            if (c != null)
            {
                c.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(DragMouseLeftButtonUp);
            }
            this.PreviewMouseMove += new MouseEventHandler(PreviewMouseMoveButtons);
        }

        /// <summary>
        /// Remove all PreviewMouseMove events from the outlookbar that have been possible set at the beginning of a drag command.
        /// </summary>
        void DragMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Control c = e.OriginalSource as Control;
            if (c != null)
            {
                c.PreviewMouseLeftButtonUp -= DragMouseLeftButtonUp;
            }
            this.PreviewMouseMove -= PreviewMouseMoveButtons;
            this.PreviewMouseMove -= PreviewMouseMoveResize;
        }

        void PreviewMouseMoveButtons(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point pos = e.GetPosition(this);
                double h = this.ActualHeight - 1 - ButtonHeight - pos.Y;
                MaxNumberOfButtons = (int)(h / ButtonHeight);
            }
            else this.PreviewMouseMove -= PreviewMouseMoveButtons;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ApplySections();
            ApplySkin();
        }

        protected void ApplySkin()
        {
            string skinName;
            switch (Skin)
            {
                case OdysseySkin.OutllookBlue: skinName = "OutlookBlueSkin"; break;
                case OdysseySkin.OutlookSilver: skinName = "OutlookSilverSkin"; break;
                case OdysseySkin.OutlookBlack: skinName = "OutlookBlackSkin"; break;
                default: skinName = "OutlookBlueSkin"; break;
            }

            if (!string.IsNullOrEmpty(skinName))
            {
                skinName = string.Format("pack://application:,,,/Odyssey;Component/Skins/OutlookBar/{0}.xaml", skinName);
                Uri uri = new Uri(skinName, UriKind.Absolute);
                ResourceDictionary skin = new ResourceDictionary();
                skin.Source = uri;
                this.Resources = skin;
            }

        }

        /// <summary>
        /// Determine the collection of MinimizedSections and MaximizedSections depending on the MaxVisibleButtons Property.
        /// </summary>
        protected virtual void ApplySections()
        {
            if (this.IsInitialized)
            {
                maximizedSections = new Collection<OutlookSection>();
                minimizedSections = new Collection<OutlookSection>();
                int max = MaxNumberOfButtons;
                int index = 0;
                int selectedIndex = SelectedSectionIndex;
                OutlookSection selectedContent = null;

                int n = GetNumberOfMinimizedButtons();

                foreach (OutlookSection e in sections)
                {
                    e.OutlookBar = this;
                    e.Height = ButtonHeight;
                    if (max-- > 0)
                    {
                        e.IsMaximized = true;
                        maximizedSections.Add(e);
                    }
                    else
                    {
                        e.IsMaximized = false;
                        if (minimizedSections.Count < n)
                        {
                            minimizedSections.Add(e);
                        }
                    }
                    bool selected = index++ == selectedIndex;
                    e.IsSelected = selected;
                    if (selected) selectedContent = e;
                }
                SetValue(OutlookBar.MaximizedSectionsPropertyKey, maximizedSections);
                SetValue(OutlookBar.MinimizedSectionsPropertyKey, minimizedSections);
                SelectedSection = selectedContent;
            }
            
        }


        private Collection<object> overflowMenu;

        /// <summary>
        /// Gets or sets the default items for the overflow menu.
        /// </summary>
        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Collection<object> OverflowMenuItems
        {
            get { return overflowMenu; }
            //    private set { overflowMenu = value; }
        }

        public static readonly DependencyProperty OverflowMenuProperty =
            DependencyProperty.Register("OverflowMenu", typeof(ItemCollection), typeof(OutlookBar), new UIPropertyMetadata(null));




        private void ApplyOverflowMenu()
        {
            Collection<object> overflowItems = new Collection<object>();
            if (OverflowMenuItems.Count > 0)
            {
                foreach (object item in OverflowMenuItems)
                {
                    overflowItems.Add(item);
                }
            }

            bool separatorAdded = false;
            int visibleButtons = maximizedSections.Count + (IsMaximized ? minimizedSections.Count : 0);

            for (int i = visibleButtons; i < sections.Count; i++)
            {
                if (!separatorAdded)
                {
                    overflowItems.Add(new Separator());
                    separatorAdded = true;
                }
                OutlookSection section = sections[i];
                MenuItem item = new MenuItem();
                item.Header = section.Header;
                Image image = new Image();
                image.Source = section.Image;
                item.Icon = image;
                item.Tag = section;
                item.Click += new RoutedEventHandler(item_Click);
                overflowItems.Add(item);
            }

            SetValue(OutlookBar.OverflowMenuItemsPropertyKey, overflowItems);
        }





        private int GetNumberOfMinimizedButtons()
        {
            if (minimizedButtonContainer != null)
            {
                const double width = 32;
                const double overflowWidth = 18;
                double fraction = (minimizedButtonContainer.ActualWidth - overflowWidth) / width;
                int minimizedButtons = (int)Math.Truncate(fraction);
                int visibleButtons = MaxNumberOfButtons + minimizedButtons;
                return visibleButtons;
            }
            return 0;
        }

        public event EventHandler<OverflowMenuCreatedEventArgs> OverflowMenuCreated;

        protected virtual void OnOverflowMenuCreated(Collection<object> menuItems)
        {
            if (OverflowMenuCreated != null)
            {
                OverflowMenuCreatedEventArgs e = new OverflowMenuCreatedEventArgs(menuItems);
                OverflowMenuCreated(this, e);
            }
        }

        void item_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = e.OriginalSource as MenuItem;
            OutlookSection section = item.Tag as OutlookSection;
            this.SelectedSection = section;
        }


        void SectionsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ApplySections();
        }

        public override void OnApplyTemplate()
        {
            minimizedButtonContainer = this.GetTemplateChild(partMinimizedButtonContainer) as FrameworkElement;

            base.OnApplyTemplate();
        }


        private ObservableCollection<OutlookSection> sections;

        /// <summary>
        /// Gets the collection of sections.
        /// </summary>
        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Collection<OutlookSection> Sections
        {
            get { return sections; }
            //            private set { sections = value as ObservableCollection<OutlookSection>; }
        }


        private ObservableCollection<OutlookSection> sideButtons = new ObservableCollection<OutlookSection>();

        /// <summary>
        /// Gets the buttons on the side when IsExpanded is set to false and ShowSideButtons is set to true.
        /// </summary>
        public Collection<OutlookSection> SideButtons
        {
            get { return sideButtons; }
        }



        /// <summary>
        /// Gets or sets whether to show the SideButtons when IsExpanded is set to false.
        /// </summary>
        public bool ShowSideButtons
        {
            get { return (bool)GetValue(ShowSideButtonsProperty); }
            set { SetValue(ShowSideButtonsProperty, value); }
        }

        public static readonly DependencyProperty ShowSideButtonsProperty =
            DependencyProperty.Register("ShowSideButtons", typeof(bool), typeof(OutlookBar), new UIPropertyMetadata(true));


        /// <summary>
        /// Gets or sets whether the Outlookbar is Maximized or Minimized.
        /// </summary>
        public bool IsMaximized
        {
            get { return (bool)GetValue(IsMaximizedProperty); }
            set { SetValue(IsMaximizedProperty, value); }
        }

        public static readonly DependencyProperty IsMaximizedProperty =
            DependencyProperty.Register("IsMaximized", typeof(bool), typeof(OutlookBar), new UIPropertyMetadata(true, MaximizedPropertyChanged));

        private static void MaximizedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OutlookBar bar = (OutlookBar)d;
            bar.OnMaximizedChanged((bool)e.NewValue);
        }

        private double previousMaxWidth = double.PositiveInfinity;

        /// <summary>
        /// Occurs when the IsMaximized property has changed.
        /// </summary>
        /// <param name="isExpanded"></param>
        protected virtual void OnMaximizedChanged(bool isExpanded)
        {
            EnsureSectionContentIsVisible();

            if (isExpanded)
            {
                MaxWidth = previousMaxWidth;
                RaiseEvent(new RoutedEventArgs(ExpandedEvent));
            }
            else
            {
                previousMaxWidth = MaxWidth;
                MaxWidth = MinimizedWidth + (CanResize ? 4 : 0);
                RaiseEvent(new RoutedEventArgs(CollapsedEvent));
            }
        }


        /// <summary>
        /// Occurs after the OutlookBar has collapsed.
        /// </summary>
        public event RoutedEventHandler Collapsed
        {
            add { AddHandler(OutlookBar.CollapsedEvent, value); }
            remove { RemoveHandler(OutlookBar.CollapsedEvent, value); }
        }

        /// <summary>
        /// Occurs after the OutlookBar has expanded.
        /// </summary>
        public event RoutedEventHandler Expanded
        {
            add { AddHandler(OutlookBar.ExpandedEvent, value); }
            remove { RemoveHandler(OutlookBar.ExpandedEvent, value); }
        }

        public static readonly RoutedEvent CollapsedEvent = EventManager.RegisterRoutedEvent("CollapsedEvent",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OutlookBar));

        public static readonly RoutedEvent ExpandedEvent = EventManager.RegisterRoutedEvent("ExpandedEvent",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OutlookBar));

        /// <summary>
        /// This code ensures that the section content is visible when the IsExpanded has changed,
        /// since the parent of the content could have changed either.
        /// </summary>
        private void EnsureSectionContentIsVisible()
        {
            object content = SectionContent;
            SectionContent = null;  // set temporarily to null, so resetting to the current content will have an effect.
            SectionContent = content;
        }


        /// <summary>
        /// Gets or sets the width when IsExpanded is set to false.
        /// </summary>
        public double MinimizedWidth
        {
            get { return (double)GetValue(MinimizedWidthProperty); }
            set { SetValue(MinimizedWidthProperty, value); }
        }

        public static readonly DependencyProperty MinimizedWidthProperty =
            DependencyProperty.Register("MinimizedWidth", typeof(double), typeof(OutlookBar), new UIPropertyMetadata((double)32));




        /// <summary>
        /// Gets or sets how to align template of the OutlookBar.
        /// Currently, only Left or Right is supported!
        /// </summary>
        public HorizontalAlignment Dock
        {
            get { return (HorizontalAlignment)GetValue(DockProperty); }
            set { SetValue(DockProperty, value); }
        }

        public static readonly DependencyProperty DockProperty =
            DependencyProperty.Register("Dock", typeof(HorizontalAlignment), typeof(OutlookBar), new UIPropertyMetadata(HorizontalAlignment.Left));

        private static readonly DependencyPropertyKey MaximizedSectionsPropertyKey =
            DependencyProperty.RegisterReadOnly("MaximizedSections", typeof(Collection<OutlookSection>), typeof(OutlookBar), new UIPropertyMetadata(null));
        public static readonly DependencyProperty MaximizedSectionsProperty = MaximizedSectionsPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey MinimizedSectionsPropertyKey =
            DependencyProperty.RegisterReadOnly("MinimizedSections", typeof(Collection<OutlookSection>), typeof(OutlookBar), new UIPropertyMetadata(null));
        public static readonly DependencyProperty MinimizedSectionsProperty = MinimizedSectionsPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey OverflowMenuItemsPropertyKey =
            DependencyProperty.RegisterReadOnly("OverflowMenuItems", typeof(Collection<object>), typeof(OutlookBar), new UIPropertyMetadata(null));
        public static readonly DependencyProperty OverflowMenuItemsProperty = OverflowMenuItemsPropertyKey.DependencyProperty;


        /// <summary>
        /// Gets or sets how many buttons are completely visible.
        /// </summary>
        public int MaxNumberOfButtons
        {
            get { return (int)GetValue(MaxNumberOfButtonsProperty); }
            set { SetValue(MaxNumberOfButtonsProperty, value); }
        }

        public static readonly DependencyProperty MaxNumberOfButtonsProperty =
            DependencyProperty.Register("MaxNumberOfButtons", typeof(int), typeof(OutlookBar),
            new FrameworkPropertyMetadata(int.MaxValue,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure,
                MaxNumberOfButtonsPropertyChanged));

        private static void MaxNumberOfButtonsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OutlookBar bar = (OutlookBar)d;
            bar.ApplySections();
        }


        /// <summary>
        /// Gets or sets whether the popup panel is visible.
        /// </summary>
        public bool IsPopupVisible
        {
            get { return (bool)GetValue(IsPopupVisibleProperty); }
            set { SetValue(IsPopupVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsPopupVisibleProperty =
            DependencyProperty.Register("IsPopupVisible", typeof(bool), typeof(OutlookBar), new UIPropertyMetadata(false, PopupVisiblePropertyChanged));

        private static void PopupVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OutlookBar bar = (OutlookBar)d;
            bar.OnPopupVisibleChanged((bool)e.NewValue);
        }

        /// <summary>
        /// Occurs when the IsPopupVisible has changed.
        /// </summary>
        /// <param name="isPopupVisible"></param>
        protected virtual void OnPopupVisibleChanged(bool isPopupVisible)
        {
            if (isPopupVisible)
            {
                RaiseEvent(new RoutedEventArgs(PopupOpenedEvent));
            }
            else
            {
                RaiseEvent(new RoutedEventArgs(PopupClosedEvent));
            }
        }

        /// <summary>
        /// Occurs after the Popup has opened.
        /// </summary>
        public event RoutedEventHandler PopupOpened
        {
            add { AddHandler(OutlookBar.PopupOpenedEvent, value); }
            remove { RemoveHandler(OutlookBar.PopupOpenedEvent, value); }
        }

        /// <summary>
        /// Occurs after the Popup has closed.
        /// </summary>
        public event RoutedEventHandler PopupClosed
        {
            add { AddHandler(OutlookBar.PopupClosedEvent, value); }
            remove { RemoveHandler(OutlookBar.PopupClosedEvent, value); }
        }

        public static readonly RoutedEvent PopupOpenedEvent = EventManager.RegisterRoutedEvent("PopupOpenedEvent",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OutlookBar));

        public static readonly RoutedEvent PopupClosedEvent = EventManager.RegisterRoutedEvent("PopupClosedEvent",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OutlookBar));

        /// <summary>
        /// Gets or sets the index of the selected section.
        /// </summary>
        public int SelectedSectionIndex
        {
            get { return (int)GetValue(SelectedSectionIndexProperty); }
            set { SetValue(SelectedSectionIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedSectionIndexProperty =
            DependencyProperty.Register("SelectedSectionIndex", typeof(int), typeof(OutlookBar), new UIPropertyMetadata(0, SelectedIndexPropertyChanged));

        private static void SelectedIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OutlookBar bar = (OutlookBar)d;
            bar.ApplySections();
        }



        /// <summary>
        /// Gets or sets the selected section.
        /// </summary>
        public OutlookSection SelectedSection
        {
            get { return (OutlookSection)GetValue(SelectedSectionProperty); }
            set { SetValue(SelectedSectionProperty, value); }
        }

        public static readonly DependencyProperty SelectedSectionProperty =
            DependencyProperty.Register("SelectedSection", typeof(OutlookSection), typeof(OutlookBar),
            new UIPropertyMetadata(null, SelectedSectionPropertyChanged));


        private static void SelectedSectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OutlookBar bar = (OutlookBar)d;
            bar.OnSelectedSectionChanged((OutlookSection)e.OldValue, (OutlookSection)e.NewValue);
        }

        /// <summary>
        /// Occurs when the SelectedSection has changed.
        /// </summary>
        protected virtual void OnSelectedSectionChanged(OutlookSection oldSection, OutlookSection newSection)
        {

            for (int index = 0; index < sections.Count; index++)
            {
                OutlookSection section = sections[index];
                bool selected = newSection == section;
                section.IsSelected = newSection == section;
                if (selected)
                {
                    SelectedSectionIndex = index;
                    SectionContent = section.Content;
                }
            }
            RaiseEvent(new RoutedPropertyChangedEventArgs<OutlookSection>(oldSection, newSection, SelectedSectionChangedEvent));
        }


        /// <summary>
        /// Occurs when the SelectedSection has changed.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<OutlookSection> SelectedSectionChanged
        {
            add { AddHandler(OutlookBar.SelectedSectionChangedEvent, value); }
            remove { RemoveHandler(OutlookBar.SelectedSectionChangedEvent, value); }
        }

        public static readonly RoutedEvent SelectedSectionChangedEvent = EventManager.RegisterRoutedEvent("SelectedSectionChangedEvent",
            RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<OutlookSection>), typeof(OutlookBar));




        /// <summary>
        /// Gets the content of the selected section.
        /// </summary>
        internal object SectionContent
        {
            get { return (object)GetValue(SectionContentProperty); }
            set { SetValue(SectionContentPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey SectionContentPropertyKey =
            DependencyProperty.RegisterReadOnly("SectionContent", typeof(object), typeof(OutlookBar), new UIPropertyMetadata(null));
        public static readonly DependencyProperty SectionContentProperty = SectionContentPropertyKey.DependencyProperty;



        /// <summary>
        /// Gets or sets whether the overflow menu of the available sections is visible.
        /// </summary>
        public bool IsOverflowVisible
        {
            get { return (bool)GetValue(IsOverflowVisibleProperty); }
            set { SetValue(IsOverflowVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsOverflowVisibleProperty =
            DependencyProperty.Register("IsOverflowVisible", typeof(bool), typeof(OutlookBar), new UIPropertyMetadata(false, OverflowVisiblePropertyChanged));


        private static void OverflowVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OutlookBar bar = (OutlookBar)d;
            bool newValue = (bool)e.NewValue;
            bar.OnOverflowVisibleChanged(newValue);
        }

        /// <summary>
        /// Occurs when the IsOverflowVisible has changed.
        /// </summary>
        /// <param name="newValue"></param>
        protected virtual void OnOverflowVisibleChanged(bool newValue)
        {

            if (newValue == true)
            {
                ApplyOverflowMenu();
            }
        }

        /// <summary>
        /// Toggles the IsExpanded property.
        /// </summary>
        public static RoutedUICommand CollapseCommand
        {
            get { return collapseCommand; }
        }

        /// <summary>
        /// Starts dragging the splitter for the visible section buttons (used for the xaml template).
        /// </summary>
        public static RoutedUICommand StartDraggingCommand
        {
            get { return startDraggingCommand; }
        }

        /// <summary>
        /// Shows the popup window.
        /// </summary>
        public static RoutedUICommand ShowPopupCommand
        {
            get { return showPopupCommand; }
        }

        /// <summary>
        /// Start to resize the Width of the OutlookBar (used for the xaml template to initiate resizing).
        /// </summary>
        public static RoutedUICommand ResizeCommand
        {
            get { return resizeCommand; }
        }

        /// <summary>
        /// Close the OutlookBar
        /// </summary>
        public static RoutedUICommand CloseCommand
        {
            get { return closeCommand; }
        }
        private static RoutedUICommand collapseCommand = new RoutedUICommand("Collapse", "CollapseCommand", typeof(OutlookBar));
        private static RoutedUICommand startDraggingCommand = new RoutedUICommand("Drag", "StartDraggingCommand", typeof(OutlookBar));
        private static RoutedUICommand showPopupCommand = new RoutedUICommand("ShowPopup", "ShowPopupCommand", typeof(OutlookBar));
        private static RoutedUICommand resizeCommand = new RoutedUICommand("Resize", "ResizeCommand", typeof(OutlookBar));
        private static RoutedUICommand closeCommand = new RoutedUICommand("Close", "CloseCommand", typeof(OutlookBar));


        /// <summary>
        /// Gets or sets the height of the section buttons.
        /// </summary>
        public double ButtonHeight
        {
            get { return (double)GetValue(ButtonHeightProperty); }
            set { SetValue(ButtonHeightProperty, value); }
        }

        public static readonly DependencyProperty ButtonHeightProperty =
            DependencyProperty.Register("ButtonHeight", typeof(double), typeof(OutlookBar), new UIPropertyMetadata((double)28.0));





        /// <summary>
        /// Gets or sets the with of the popup window.
        /// </summary>
        public double PopupWidth
        {
            get { return (double)GetValue(PopupWidthProperty); }
            set { SetValue(PopupWidthProperty, value); }
        }

        public static readonly DependencyProperty PopupWidthProperty =
            DependencyProperty.Register("PopupWidth", typeof(double), typeof(OutlookBar), new UIPropertyMetadata((double)double.NaN));




        /// <summary>
        /// Gets or sets whether the splitter for the section buttons is visible
        /// </summary>
        public bool IsButtonSplitterVisible
        {
            get { return (bool)GetValue(IsButtonSplitterVisibleProperty); }
            set { SetValue(IsButtonSplitterVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsButtonSplitterVisibleProperty =
            DependencyProperty.Register("IsButtonSplitterVisible", typeof(bool), typeof(OutlookBar), new UIPropertyMetadata(true));


        /// <summary>
        /// Gets or sets whether the section buttons are visible.
        /// </summary>
        public bool ShowButtons
        {
            get { return (bool)GetValue(ShowButtonsProperty); }
            set { SetValue(ShowButtonsProperty, value); }
        }

        public static readonly DependencyProperty ShowButtonsProperty =
            DependencyProperty.Register("ShowButtons", typeof(bool), typeof(OutlookBar), new UIPropertyMetadata(true));


        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Collection<ButtonBase> OptionButtons
        {
            get { return (Collection<ButtonBase>)GetValue(OptionButtonsProperty); }
            //  private set { SetValue(OptionButtonsProperty, value); }
        }

        private static readonly DependencyPropertyKey OptionButtonsPropertyKey =
            DependencyProperty.RegisterReadOnly("OptionButtons", typeof(Collection<ButtonBase>), typeof(OutlookBar), new UIPropertyMetadata(null));
        public static readonly DependencyProperty OptionButtonsProperty = OptionButtonsPropertyKey.DependencyProperty;




        /// <summary>
        /// Gets or sets wether the width of the OutlookBar can be manually resized by a gripper at the right (or left).
        /// </summary>
        public bool CanResize
        {
            get { return (bool)GetValue(CanResizeProperty); }
            set { SetValue(CanResizeProperty, value); }
        }

        public static readonly DependencyProperty CanResizeProperty =
            DependencyProperty.Register("CanResize", typeof(bool), typeof(OutlookBar), new UIPropertyMetadata(true));



        /// <summary>
        /// Gets or sets wether the close button is visible.
        /// </summary>
        public bool IsCloseButtonVisible
        {
            get { return (bool)GetValue(IsCloseButtonVisibleProperty); }
            set { SetValue(IsCloseButtonVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsCloseButtonVisibleProperty =
            DependencyProperty.Register("IsCloseButtonVisible", typeof(bool), typeof(OutlookBar), new UIPropertyMetadata(false));



        /// <summary>
        /// Gets or sets the text or content that is displayed on the minimized OutlookBar at the Button to open up the Navigation Pane.
        /// </summary>
        public object NavigationPaneText
        {
            get { return (object)GetValue(NavigationPaneTextProperty); }
            set { SetValue(NavigationPaneTextProperty, value); }
        }

        public static readonly DependencyProperty NavigationPaneTextProperty =
            DependencyProperty.Register("NavigationPaneText", typeof(object), typeof(OutlookBar), new UIPropertyMetadata("Navigation Pane"));



        /// <summary>
        /// Gets or sets the desired skin.
        /// </summary>            
        public OdysseySkin Skin
        {
            get { return (OdysseySkin)GetValue(SkinProperty); }
            set { SetValue(SkinProperty, value); }
        }

        public static readonly DependencyProperty SkinProperty =
            DependencyProperty.Register("Skin", typeof(OdysseySkin), typeof(OutlookBar),
            new FrameworkPropertyMetadata(OdysseySkin.Default, FrameworkPropertyMetadataOptions.AffectsRender, SkinPropertyPropertyChanged));


        private static void SkinPropertyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OutlookBar bar = (OutlookBar)d;
            OdysseySkin skin = (OdysseySkin)e.NewValue;
            bar.OnSkinChanged(skin);
        }

        protected virtual void OnSkinChanged(OdysseySkin skin)
        {
            ApplySkin();
        }

    }
}
