using System;
using System.Collections.ObjectModel;
using System.Data;

namespace ADORE.Standard
{
	public class CommandParameterCollection : Collection<CommandParameter>
	{
		public CommandParameter this[string name]
		{
			get
			{
				for(int x = 0; x < this.Items.Count; x++)
				{
					if(this.Items[x].Name == name) { return this.Items[x]; }
				}
				return null;
			}
			set
			{
				for(int x = 0; x < this.Items.Count; x++)
				{
					if(this.Items[x].Name == name)
					{
						this.Items[x] = value;
						return;
					}
				}
				this.Add(value);
			}
		}

		/// <summary>
		/// <para>Add a CommandParameter to the collection piece-by-piece.</para>
		/// </summary>
		/// <param name="name">The parameter name</param>
		/// <param name="value">The parameter value</param>
		/// <param name="type">The parameter data type</param>
		/// <param name="direction">The parameter direction</param>
		/// <returns>A reference to this collection, allowing chained calls to this method.</returns>
		public CommandParameterCollection Add(string name, object value, DbType type, ParameterDirection direction)
		{
			this.Add(new CommandParameter() { Name = name, Value = value, Type = type, Direction = direction });
			return this;
		}
	}
}
