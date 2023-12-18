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

namespace ViewPane.Toolbar
{
	[SmartAssembly.Attributes.DoNotPrune]
	[SmartAssembly.Attributes.DoNotPruneType]
	[SmartAssembly.Attributes.DoNotObfuscate]
	[SmartAssembly.Attributes.DoNotObfuscateType]
	public class ToolbarComboBox : ComboBox
	{
		#region ToolbarComboBox()
		static ToolbarComboBox()
		{
			//DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolbarComboBox), new FrameworkPropertyMetadata(typeof(ToolbarComboBox)));
		}

		public ToolbarComboBox()
		{
			this.Width = 100;
			this.Height = 23;
		}
		#endregion

		public class ComboItem
		{
			public object Tag;
			public string Text;

			public ComboItem(object tag, string text)
			{
				this.Tag = tag;
				this.Text = text;
			}

			public override string ToString()
			{
				return this.Text;
			}
		}

		//PUBLIC PROPERTIES
		#region public properties

		/*new public object SelectedItem
		{
			get
			{
				if (base.SelectedItem != null)
					return ((ComboItem)base.SelectedItem).Tag;
				else
					return null;
			}
			set
			{
				ComboItem comboItem = GetItem(value);

				if (comboItem != null)
					base.SelectedItem = comboItem;
			}
		}*/

		#endregion

		//PUBLIC METHODS
		#region public methods

		#region AddItem()
		public void AddItem(object item)
		{
			this.Items.Add(item);
		}
		#endregion

		#region AddSeparator()
		public void AddSeparator()
		{
			this.Items.Add("-");
		}
		#endregion

		#region RemoveItem()
		public void RemoveItem(object item)
		{
			ComboItem comboItem = GetItem(item);

			if (comboItem != null)
				this.Items.Remove(comboItem);
		}
		#endregion

		#region Clear()
		public void Clear()
		{
			this.Items.Clear();
		}
		#endregion

		#endregion

		//PRIVATE METHODS
		#region private methods

		#region GetItem()
		ComboItem GetItem(object item)
		{
			foreach (ComboItem comboItem in this.Items)
				if (comboItem.Tag == item)
					return comboItem;

			return null;
		}
		#endregion

		#endregion

	}
}
