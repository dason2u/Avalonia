﻿// -----------------------------------------------------------------------
// <copyright file="DeckPresenter.cs" company="Steven Kirk">
// Copyright 2015 MIT Licence. See licence.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace Perspex.Controls.Presenters
{
    using Perspex.Animation;
    using Perspex.Controls.Generators;
    using Perspex.Controls.Primitives;
    using Perspex.Input;
    using Perspex.Styling;
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    public class DeckPresenter : Control, IVisual, IPresenter, ITemplatedControl
    {
        public static readonly PerspexProperty<IEnumerable> ItemsProperty =
            ItemsControl.ItemsProperty.AddOwner<DeckPresenter>();

        public static readonly PerspexProperty<ItemsPanelTemplate> ItemsPanelProperty =
            ItemsControl.ItemsPanelProperty.AddOwner<DeckPresenter>();

        public static readonly PerspexProperty<object> SelectedItemProperty =
            SelectingItemsControl.SelectedItemProperty.AddOwner<DeckPresenter>();

        public static readonly PerspexProperty<IVisibilityTransition> TransitionProperty =
            Deck.TransitionProperty.AddOwner<DeckPresenter>();

        private bool createdPanel;

        public DeckPresenter()
        {
            this.GetObservableWithHistory(SelectedItemProperty).Subscribe(this.SelectedItemChanged);
        }

        public ItemContainerGenerator ItemContainerGenerator
        {
            get;
            private set;
        }

        public IEnumerable Items
        {
            get { return this.GetValue(ItemsProperty); }
            set { this.SetValue(ItemsProperty, value); }
        }

        public ItemsPanelTemplate ItemsPanel
        {
            get { return this.GetValue(ItemsPanelProperty); }
            set { this.SetValue(ItemsPanelProperty, value); }
        }

        public object SelectedItem
        {
            get { return this.GetValue(SelectedItemProperty); }
            set { this.SetValue(SelectedItemProperty, value); }
        }

        public Panel Panel
        {
            get;
            private set;
        }

        public IVisibilityTransition Transition
        {
            get { return this.GetValue(TransitionProperty); }
            set { this.SetValue(TransitionProperty, value); }
        }

        public override sealed void ApplyTemplate()
        {
            if (!this.createdPanel)
            {
                this.CreatePanel();
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            this.Panel.Measure(availableSize);
            return this.Panel.DesiredSize.Value;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this.Panel.Arrange(new Rect(finalSize));
            return finalSize;
        }

        private void CreatePanel()
        {
            this.ClearVisualChildren();
            this.Panel = this.ItemsPanel.Build();
            this.Panel.TemplatedParent = this;
            KeyboardNavigation.SetTabNavigation(this.Panel, KeyboardNavigationMode.Once);
            ((IItemsPanel)this.Panel).ChildLogicalParent = this.TemplatedParent as ILogical;
            this.AddVisualChild(this.Panel);
            this.createdPanel = true;
            this.SelectedItemChanged(Tuple.Create<object, object>(null, this.SelectedItem));
        }

        private IItemContainerGenerator GetGenerator()
        {
            if (this.ItemContainerGenerator == null)
            {
                ItemsControl i = this.TemplatedParent as ItemsControl;
                this.ItemContainerGenerator = i?.ItemContainerGenerator ?? new ItemContainerGenerator(this);
            }

            return this.ItemContainerGenerator;
        }

        private async void SelectedItemChanged(Tuple<object, object> value)
        {
            if (this.createdPanel)
            {
                var generator = this.GetGenerator();
                Control from = null;
                Control to = null;

                if (value.Item1 != null)
                {
                    from = generator.GetContainerForItem(value.Item1);
                }

                if (value.Item2 != null)
                {
                    to = generator.Generate(new[] { value.Item2 }).Single();
                    this.Panel.Children.Add(to);
                }

                if (this.Transition != null)
                {
                    await this.Transition.Start(from, to);
                }

                if (from != null)
                {
                    this.Panel.Children.Remove(from);
                    generator.Remove(new[] { value.Item1 });
                }
            }
        }
    }
}