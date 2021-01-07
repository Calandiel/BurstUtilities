using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using Calandiel.Collections;
using System.IO;

namespace Calandiel.Utilities
{
	public static class SavingUtility
	{
		public static void Save<T>(T obj, BinaryWriter writer) where T: unmanaged
		{
			Save((object)obj, writer);
		}
		public static T Load<T>(BinaryReader reader) where T : unmanaged
		{
			var obj = (object)default(T);
			Load(ref obj, reader);
			return (T)obj;
		}

		public static void Save(object obj, BinaryWriter writer)
		{
			// First, check if we're saving a value type.
			// Also, make sure we're not saving a pointer.
			var type = obj.GetType();
			if (type.IsValueType || type.IsPointer)
			{
				switch (obj)
				{
					#region PRIMITIVES
					case float fl:
						writer.Write(fl);
						break;
					case double db:
						writer.Write(db);
						break;
					case bool bl:
						writer.Write(bl);
						break;
					case byte b:
						writer.Write(b);
						break;
					case sbyte sb:
						writer.Write(sb);
						break;
					case ushort us:
						writer.Write(us);
						break;
					case short s:
						writer.Write(s);
						break;
					case uint ui:
						writer.Write(ui);
						break;
					case int i:
						writer.Write(i);
						break;
					case ulong ul:
						writer.Write(ul);
						break;
					case long l:
						writer.Write(l);
						break;
					#endregion
					default:
						// If we get here, the value is either a collection or a structure.
						if (type.IsEnum)
						{
							var val = Convert.ToInt32(obj);
							writer.Write(val);
						}
						// if it's not an enum, it's either a struct (in which case, we need recursion)
						// or a collection (in which case, we need to find out what type of collection it is, so that we can loop over them
						else if (type.GetCustomAttribute<UnmanagedCollectionAttribute>() != null)
						{
							var iucs = obj as IUnmanagedCollectionSave;
							iucs.Save(writer);
						}
						else
						{
							var vars = type.GetFields();
							foreach (var v in vars)
							{
								// statics could easily cause infinite recursion
								if (v.IsStatic == false)
								{
									var o = v.GetValue(obj);
									Save(o, writer);
								}
							}
						}
						break;
				}
			}
			else
			{
				ErrorManager.ThrowWithThrow($"{type.Name} IS EITHER A POINTER OR NOT A VALUE TYPE! WE CAN'T SAVE IT!");
			}
		}

		public static void Load(ref object obj, BinaryReader reader)
		{
			var type = obj.GetType();
			if (type.IsValueType || type.IsPointer)
			{
				switch(obj)
				{
					#region PRIMITIVES
					case float _:
						obj = reader.ReadSingle();
						break;
					case double _:
						obj = reader.ReadDouble();
						break;
					case bool _:
						obj = reader.ReadBoolean();
						break;
					case byte _:
						obj = reader.ReadByte();
						break;
					case sbyte _:
						obj = reader.ReadSByte();
						break;
					case ushort _:
						obj = reader.ReadUInt16();
						break;
					case short _:
						obj = reader.ReadInt16();
						break;
					case uint _:
						obj = reader.ReadUInt32();
						break;
					case int _:
						obj = reader.ReadInt32();
						break;
					case ulong _:
						obj = reader.ReadUInt64();
						break;
					case long _:
						obj = reader.ReadInt64();
						break;
					#endregion
					default:
						// If we get here, the value is either a collection or a structure.
						if (type.IsEnum)
						{
							obj = System.Enum.Parse(type, reader.ReadInt32().ToString());
						}
						// if it's not an enum, it's either a struct (in which case, we need recursion)
						// or a collection (in which case, we need to find out what type of collection it is, so that we can loop over them
						else if (type.GetCustomAttribute<UnmanagedCollectionAttribute>() != null)
						{
							var iucs = obj as IUnmanagedCollectionSave;
							iucs.Load(reader);
							obj = iucs;
						}
						else
						{
							var vars = type.GetFields();
							foreach (var v in vars)
							{
								// statics could easily cause infinite recursion
								if (v.IsStatic == false)
								{
									var o = v.GetValue(obj);
									Load(ref o, reader);
									v.SetValue(obj, o);
								}
							}
						}
						break;
				}
			}
			else
			{
				ErrorManager.ThrowWithThrow($"{type.Name} IS EITHER A POINTER OR NOT A VALUE TYPE! WE CAN'T LOAD IT!");
			}
		}
	}
}