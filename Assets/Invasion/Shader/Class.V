Header <MyClass>

Import <Base.Windows.Framework>

Namespace Invasion : Definition
{
	Public Class MyClass : {Any, System, Class}
	{
		Public [Shift: <Any>, Return: <Boolean>] Print{<input, String>, (VARIATIC_ARGUMENTS, {<inputOther...>}, "Base.Lang.String")}
		{
			Windows.Print("Hello World!");
		}
	}

	[<SysCall>, "user32.dll", <Any>]
	Public [Shift: <Any>, Return: <Boolean>] MessageBox{<SysDef>};
}