#region Usings
using System;
using System.IO;
using System.Collections;
using System.Management;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using System.Text.RegularExpressions;
#endregion

namespace MediaPortal.GUI.GUIScript
{
	/// <summary>
	/// Summary description for MPScript.
	/// </summary>
	public class MPScript
	{
		#region Private Variables
		// funktions token
		private const char tStatic='@';
		private const char tGlobal='°';
		private const char tVar='@';
		private const char tIf='a';
		private const char tEndIf='b';	
		private const char tElse='c';
		private const char tWhile='d';
		private const char tSwitch='e';
		private const char tFor='f';
		private const char tMp_action='g';
		private const char tEnd='h';
		private const char tEndWhile='i';
		private const char tEndSwitch='j';
		private const char tCase='k';
		private const char tMessageBox='l';
		private const char tEndFor='m';
		private const char tCall='n';
		private const char tDefault='o';

		// regular expression 
		Regex rexStripOp = new Regex(@"\+|-|\*|/|%|&&|\|\||&|\||\^|==|!=|>=|<=|=|<|>|!|\(|\)|\:|true|false|,", RegexOptions.IgnoreCase);
		Regex rexStripQuote = new Regex("\"", RegexOptions.IgnoreCase);
		Regex rexStripString = new Regex( "\\\"(?<String>.*?[^\\\\])\\\"", RegexOptions.IgnoreCase);
 		
		// static vars
		private static string strTokenbuffer;
		private static char[] DELIMITER = {' ','(','=','\"',')' };
		private static char[] BYPASSCODE = {'\t',' ','(','\"',')','=','%','§','@','$','°' };
		private static Hashtable global_variable  = new Hashtable();
		private static int numScripts=0;

		private char oper=' '; 
		private String[] scriptArray;
		private int progPointer=0;
		private int ScriptEnd=0;
		private long BreakTime=5000;
		private string ScriptName="";

		// hashtables for vars and functions
		private Hashtable tokens = new Hashtable();
		private Hashtable variable = new Hashtable();
		private Hashtable functions = new Hashtable();
		
		// stack
		private Stack stack = new Stack();

		ExpressionEval eval = new ExpressionEval();

		#endregion

		#region Constructor
		public MPScript()
		{
			eval.FunctionHandler += new FunctionHandler(MPFunctions);
			
			// save Tokens in hashtable
			tokens.Add("static",tStatic);
			tokens.Add("var",tVar);
			tokens.Add("global",tGlobal);
			tokens.Add("end",tEnd);
			tokens.Add("if",tIf);
			tokens.Add("endif",tEndIf);
			tokens.Add("else",tElse);
			tokens.Add("while",tWhile);
			tokens.Add("endwhile",tEndWhile);
			tokens.Add("switch",tSwitch);
			tokens.Add("case",tCase);
			tokens.Add("default",tCase);
			tokens.Add("endswitch",tEndSwitch);
			tokens.Add("for",tFor);
			tokens.Add("call",tCall);
			tokens.Add("mp_action",tMp_action);
			tokens.Add("messagebox",tMessageBox);

			// save functions in hashtable
			functions.Add("yesnobox",'A');
			functions.Add("sin",'B');
			functions.Add("cos",'C');
			functions.Add("tan",'D');
			functions.Add("asin",'E');
			functions.Add("acos",'F');
			functions.Add("atan",'G');
			functions.Add("sinh",'H');
			functions.Add("cosh",'I');
			functions.Add("tanh",'J');
			functions.Add("abs",'K');
			functions.Add("sqrt",'L');
			functions.Add("ciel",'M');
			functions.Add("floor",'N');
			functions.Add("exp",'O');
			functions.Add("log10",'P');
			functions.Add("log",'Q');
			functions.Add("max",'R');
			functions.Add("min",'S');
			functions.Add("pow",'T');
			functions.Add("round",'U');
			functions.Add("e",'V');
			functions.Add("pi",'W');
			functions.Add("now",'X');
			functions.Add("today",'Y');
			functions.Add("calc",'2');
			functions.Add("rnd",'3');
			functions.Add("random",'4');
		}
		#endregion

		#region Public helpers
		/// <summary>
		/// sets a global var
		/// </summary>
		public void setGlobalVar(string key,object val)
		{
			if (global_variable.ContainsKey(key)) 
			{
				global_variable[key]=val;
			} 
			else 
			{
				global_variable.Add(key,val);
			}
		}
		#endregion

		#region Load Script in Memory
		/// <summary>
		/// Reads the script into Memory and make preprocessing
		/// </summary>
		/// <returns>A string containing the button text.</returns>
		// TODO: Add more commands and funktions
		public string GetScript(string name)   // Read the MPScript
		{
			numScripts++;
			string work="";
			string scratch="";
			string buttonText="";
			string scriptdir=System.IO.Directory.GetCurrentDirectory()+"\\"+"scripts";
			try 
			{
				StreamReader sr = new StreamReader(scriptdir+"\\"+name+".mps");
				String fileContent = sr.ReadToEnd();
				char[] separator = {'\n'};
				scriptArray = fileContent.Split(separator);
				ScriptName=name;
				global_variable.Add(ScriptName,"Load");

				//--------------------------------------------------------------------------
				for (int i=0;i<scriptArray.Length;i++) // little preprocessor
				{
					scriptArray[i]=scriptArray[i].Trim();
					work=scriptArray[i];
					if (work.Length<2) 
					{
						scriptArray[i]="#";
						continue;
					}
					work=work.Trim().ToLower();
					if (work.StartsWith("#description:") ==true) 
					{
						scriptArray[i]="#";
						continue;
					}
					if (work.StartsWith("#button:") ==true) 
					{
						buttonText=checkText(scriptArray[i].Trim().Substring(8));
						scriptArray[i]="#";
						continue;
					}
					if (work.StartsWith("#breaktime:") ==true) 
					{
						string tx=work.Substring(11);
						try 
						{
							int bx=Convert.ToInt32(tx);
							BreakTime=bx*1000;
						} 
						catch(Exception ex) 
						{
							BreakTime=5000;
						}
						scriptArray[i]="#";
						continue;
					}
					int indx=work.IndexOf("\"",0);
					if (indx>0)
					{
						string w=work.Substring(0,indx);
						w=w+scriptArray[i].Substring(indx);
						scriptArray[i]=w;
					} 
					else 
					{
						scriptArray[i]=work;
					}
					indx=work.IndexOf("//");  // any remarks?
					if (indx!=-1) 
					{
						if (indx==0) 
						{
							scriptArray[i]="#";
							continue;
						} 
						else 
						{
							scriptArray[i]=work.Substring(0,indx);
						}
					}
					//------------------------------------------------------------
					// Replace any command with tokens
					string tok=ParseToken(scriptArray[i]);
					if (variable.ContainsKey(tok))                  // is token a variable?
					{
						scriptArray[i]=tVar+tok+" "+changeFunc(strTokenbuffer);
					}
					if (global_variable.ContainsKey(tok)) 
					{
						scriptArray[i]=tGlobal+tok+" "+changeFunc(strTokenbuffer);
					}
					if (tokens.ContainsKey(tok))									// is token a command
					{
						switch (tok) 
						{
							case "static" :
								scriptArray[i]="#";
								string k=ParseToken(strTokenbuffer);
								if (strTokenbuffer.IndexOf("*",0)!=-1 || strTokenbuffer.IndexOf("/",0)!=-1 || strTokenbuffer.IndexOf("+",0)!=-1 || strTokenbuffer.IndexOf("-",0)!=-1) 
								{
									eval.Expression = strTokenbuffer;
									strTokenbuffer=eval.Evaluate().ToString();
								}
								string b=ParseToken(strTokenbuffer);
								variable.Add(k,b);
								break;
							case "global" :
								k=ParseToken(strTokenbuffer);
								if (strTokenbuffer.IndexOf("*",0)!=-1 || strTokenbuffer.IndexOf("/",0)!=-1 || strTokenbuffer.IndexOf("+",0)!=-1 || strTokenbuffer.IndexOf("-",0)!=-1) 
								{
									eval.Expression = strTokenbuffer;
									strTokenbuffer=eval.Evaluate().ToString();
								}
								b=ParseToken(strTokenbuffer);
								scriptArray[i]=tGlobal+k+"="+b;
								global_variable.Add(k,b);
								break;
							case "var" :
								k=ParseToken(strTokenbuffer);
								if (strTokenbuffer.IndexOf("*",0)!=-1 || strTokenbuffer.IndexOf("/",0)!=-1 || strTokenbuffer.IndexOf("+",0)!=-1 || strTokenbuffer.IndexOf("-",0)!=-1) 
								{
									eval.Expression = strTokenbuffer;
									strTokenbuffer=eval.Evaluate().ToString();
								}
								b=ParseToken(strTokenbuffer);
								scriptArray[i]=tVar+k+"="+b;
								variable.Add(k,b);
								break;
							case "if" :
								scriptArray[i]="§"+tokens[tok]+changeFunc(strTokenbuffer);
								break;
							case "endif" :
								scriptArray[i]="§"+tokens[tok];
								break;
							case "else" :
								scriptArray[i]="§"+tokens[tok];
								break;
							case "while" :
								scriptArray[i]="§"+tokens[tok]+changeFunc(strTokenbuffer);
								break;
							case "endwhile" :
								scriptArray[i]="§"+tokens[tok];
								break;
							case "switch" :
								scriptArray[i]="§"+tokens[tok]+changeFunc(strTokenbuffer);
								break;
							case "case" :
								scriptArray[i]="§"+tokens[tok]+changeFunc(strTokenbuffer);
								break;
							case "default" :
								scriptArray[i]="§"+tokens[tok];
								break;
							case "endswitch" :
								scriptArray[i]="§"+tokens[tok];
								break;
							case "for" :
								scriptArray[i]="§"+tokens[tok]+changeFunc(strTokenbuffer);
								break;
							case "messagebox" :
								scriptArray[i]="§"+tokens[tok]+changeFunc(strTokenbuffer);								
								break;
							case "call" :
								scratch=changeFunc(strTokenbuffer);
								scratch = rexStripOp.Replace(scratch,"");
								scratch = rexStripQuote.Replace(scratch,"");
								scriptArray[i]="§"+tokens[tok]+scratch.Trim();	
								ScriptHandler gScript=new ScriptHandler();
								scratch=gScript.LoadScript(scratch.Trim());
								break;
							case "mp_action" :
								scriptArray[i]="§"+tokens[tok]+ParseToken(strTokenbuffer);
								break;
							case "end" :
								scriptArray[i]="§"+tokens[tok];
								break;
						}
					}
				}
			} 
			catch(Exception ex)
			{
				Log.Write("Syntax Error!", ex.Message);
				buttonText="SCRIPT ERROR!!";
				global_variable[ScriptName]="ERROR";
			}
			global_variable[ScriptName]="Loaded";
			return buttonText;
		}
		#endregion

		#region Run MPScript
		/// <summary>
		/// Starts the Script 
		/// </summary>
		// TODO: Add more commands and funktions
		public bool RunScript()
		{		
			bool isOper=false;
			string progLine="";
			string scratch="";
			string leftvar="";
			long lStartTime = DateTime.Now.Ticks;
			long lDiff = 0;

			ScriptEnd=scriptArray.Length;
			try 
			{
				global_variable[ScriptName]="Run";
				while (progPointer<ScriptEnd) 
				{
					if (BreakTime>0) 
					{
						lDiff = (DateTime.Now.Ticks - lStartTime)/10000;
						if(lDiff>BreakTime) 
						{
							global_variable[ScriptName]="Time Break";
							return false;						
						}
					}
					isOper=false;
					progLine=scriptArray[progPointer];
					if (progLine.Substring(0,1)=="#") // Nothing to do
					{
						progPointer++;
						continue;
					}
					if (progLine.Substring(0,1)==tVar.ToString()) // var first
					{
						leftvar=ParseToken(progLine);
						if (variable.ContainsKey(leftvar)) 
						{
							if (oper=='=')								// assignment
							{
								progLine=ChangeVar(strTokenbuffer);
							{
								if (progLine=="=") 
								{
									variable[leftvar]="";
								} 
								else 
								{	
									eval.Expression = progLine;
									progLine=eval.Evaluate().ToString();
									variable[leftvar]=progLine;
								}
								progPointer++;
								continue;
							}
							}
						}
					}
					if (progLine.Substring(0,1)==tGlobal.ToString()) // global var first
					{
						leftvar=ParseToken(progLine);
						if (global_variable.ContainsKey(leftvar)) 
						{
							if (oper=='=')								// assignment
							{
								progLine=ChangeVar(strTokenbuffer);
							{
								if (progLine=="=") 
								{
									variable[leftvar]="";
								} 
								else 
								{
									eval.Expression = progLine;
									progLine=eval.Evaluate().ToString();
									global_variable[leftvar]=progLine;
								}
								progPointer++;
								continue;
							}
							}
						}
					}
					if (progLine.Substring(0,1)=="§") // command processing
					{
						char ch = Convert.ToChar(progLine.Substring(1,1));
						if (progLine.Length>2) 
						{
							progLine=progLine.Substring(2);
							progLine=ChangeVar(progLine);
						}
						switch (ch)
						{
								// If statement  ------------------------------------------------
							case tIf:
								eval.Expression = progLine;
								progLine=eval.Evaluate().ToString();
								if(progLine=="True") 
								{

									progPointer++;
								} 
								else 
								{
									progPointer++;
									jump(tElse,tIf,tEndIf);
								}
								break;
								// While statement  -----------------------------------------------
							case tWhile:
								eval.Expression = progLine;
								progLine=eval.Evaluate().ToString();
								if(progLine=="True") 
								{
									stack.Push(progPointer++);
								} 
								else 
								{
									progPointer++;
									jump(tWhile,tWhile,tEndWhile);
								}
								break;
								// Switch statement  ----------------------------------------------
							case tSwitch:
								eval.Expression = progLine;
								progLine=eval.Evaluate().ToString();
								searchCase(progLine);
								break;
								// Case statement  ------------------------------------------------
							case tCase:
								progPointer++;
								jump(tSwitch,tSwitch,tEndSwitch);
								break;								
								// EndSwitch statement  -------------------------------------------
							case tEndSwitch:
								progPointer++;
								break;								
								// EndWhile statement  --------------------------------------------
							case tEndWhile:
								progPointer=Convert.ToInt32(stack.Pop());
								break;								
								// End statement  -------------------------------------------------
							case tEnd:															
								global_variable[ScriptName]="End";
								return true;
								// Call statement -------------------------------------------------
							case tCall:
								ScriptHandler gScript=new ScriptHandler();
								global_variable[ScriptName]="Call";
								gScript.StartScriptName(progLine);
								progPointer++;
								break;
								// MP_Action statement --------------------------------------------
							case tMp_action:
								callAction(progLine);
								progPointer++;
								break;
								// MessageBox statement -------------------------------------------
							case tMessageBox:											
								DialogBox(checkText(progLine.Substring(2)));
								break;
							default:
								progPointer++;
								break;
						}
					}		
					else 
					{
						Log.Write("MPScript: Syntax error in Line {0}",progPointer);
						progPointer++;
					}
				}
			}
			catch(Exception ex)
			{
				global_variable[ScriptName]="ERROR";
				Log.Write("Syntax Error!", ex.Message);
			}

			return true;
		}
		#endregion

		#region MPScript helpers

		private string changeFunc(string buffer)
		{
			string buf="";
			string tok="";
			int i=0;

			buf = rexStripString.Replace(buffer,"");
			buf = rexStripOp.Replace(buf," ");

			strTokenbuffer=buf.Trim();
			while (strTokenbuffer.Length>=1) 
			{
				tok=ParseToken(strTokenbuffer);
				if(functions.ContainsKey(tok)) 
				{
					i=buffer.IndexOf(tok,0);
					buffer=buffer.Substring(0,i)+"$"+buffer.Substring(i);
					continue;
				}
				if(variable.ContainsKey(tok)) 
				{
					i=buffer.IndexOf(tok,0);
					buffer=buffer.Substring(0,i)+tVar+buffer.Substring(i);
					continue;
				}
				if(global_variable.ContainsKey(tok)) 
				{
					i=buffer.IndexOf(tok,0);
					buffer=buffer.Substring(0,i)+tGlobal+buffer.Substring(i);
					continue;
				}
			}
			return buffer;
		}

		private string ChangeVar(string buffer)
		{
			string buf=buffer;
			string buf2="";
			string tok;

			int i=buffer.IndexOf(tGlobal.ToString());
			while(i>-1) 
			{
				buf = buffer;
				buf = rexStripOp.Replace(buf," ");
				buf2=buffer.Substring(0,i);
				tok=ParseToken(buf.Substring(i));
				buffer=buf2+" "+global_variable[tok]+buffer.Substring(i+tok.Length+1);
				i=buffer.IndexOf(tGlobal.ToString());
			}
			i=buffer.IndexOf(tVar.ToString());
			while(i>-1) 
			{
				buf = buffer;
				buf = rexStripOp.Replace(buf," ");
				buf2=buffer.Substring(0,i);
				tok=ParseToken(buf.Substring(i));
				buffer=buf2+" "+variable[tok]+buffer.Substring(i+tok.Length+1);
				i=buffer.IndexOf(tVar.ToString());
			}
			return buffer;
		}
		
		private void searchCase(string cond)
		{
			string scratch;
			string buffer;
			while (progPointer<ScriptEnd) 
			{
				if (scriptArray[progPointer].Substring(0,1)=="§") 
				{
					if (Convert.ToChar(scriptArray[progPointer].Substring(1,1))==tCase)
					{
						buffer=scriptArray[progPointer].Substring(1);
						scratch=ParseToken(buffer);
						buffer=ParseToken(strTokenbuffer);
						buffer=rexStripOp.Replace(buffer," ");
						buffer=buffer.Trim();
						if (buffer==cond) 
						{	
							progPointer++;
							break;
						}
					}
				}
				progPointer++;
			} 
		}
		
		private void jump(char tst,char skip,char end)
		{
			int skipCnt=0;
			while (progPointer<ScriptEnd) 
			{
				if (scriptArray[progPointer].Substring(0,1)=="§") 
				{
					if (Convert.ToChar(scriptArray[progPointer].Substring(1,1))==skip)
					{
						skipCnt++;
						progPointer++;
						continue;
					}
					if (Convert.ToChar(scriptArray[progPointer].Substring(1,1))==end)
					{
						if (skipCnt==0) 
						{
							progPointer++;
							break;	
						} 
						else 
						{
							skipCnt--;
							progPointer++;
							continue;
						}
					}
					if (Convert.ToChar(scriptArray[progPointer].Substring(1,1))==tst) 
					{
						if(skipCnt==0) 
						{
							progPointer++;
							break;
						}
					} 
				}
				progPointer++;
			}
		}

		/// <summary>
		/// Reads the next token and operant
		/// </summary>
		/// <returns>A string containing the next token</returns>
		private string ParseToken(string line)
		{
			int i = 0;
			int j = line.Length;
			bool foundFg = false;

			char[] p;
			int k;

			p = line.ToCharArray();

			do
			{
				for(int ii = 0; ii < BYPASSCODE.Length; ii++)
				{
					if( p[i] == BYPASSCODE[ii] )
					{
						foundFg = true;
						break;
					}
				}
				if( foundFg )
				{
					i++;
					foundFg = false;
				}
				else
					break;
			} while(i < j);

			k = i;
			int d=0;
			while(i < j && p[i] != DELIMITER[d])
			{
				if (d<DELIMITER.Length-1) 
				{
					d++;
				} 
				else 
				{
					i++;
					d=0;
				}
			}
			oper=DELIMITER[d];
			if (oper==' ') 
			{
				int ii=i;
				while(ii < j && p[ii]==' ') 
				{
					ii++;
				}
				if (ii<j) 
				{
					oper=p[ii];
				}
			}
			strTokenbuffer = line.Substring(i,  j-i);

			return(line.Substring(k, i-k));
		}
		#endregion
	
		#region MPScript functions
		private void DialogBox(string text)
		{
			GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
			dlgOk.SetHeading(""); 
			dlgOk.SetLine(1,text);
			dlgOk.DoModal(GUIWindowManager.ActiveWindow);
		}

		private void callAction(string ac)
		{
			Action act;
			switch (ac)
			{
				case "reboot" :
					act = new Action(Action.ActionType.ACTION_REBOOT,0,0);
					GUIGraphicsContext.OnAction(act);
					break;
				case "shutdown" :
					act = new Action(Action.ActionType.ACTION_SHUTDOWN,0,0);
					GUIGraphicsContext.OnAction(act);
					break;
				case "ejectcd" :
					act = new Action(Action.ActionType.ACTION_EJECTCD,0,0);
					GUIGraphicsContext.OnAction(act);
					break;
				case "previous_menu" :
					act = new Action(Action.ActionType.ACTION_PREVIOUS_MENU,0,0);
					GUIGraphicsContext.OnAction(act);
					break;
			}
		}
		#endregion

		#region Private Static Functions

		/// <summary>
		/// Call user functions
		/// </summary>
		private static object MPFunctions(string strName, object[] a_params)
		{
			switch (strName.ToLower())
			{
				case "yesnobox" : string t=a_params[0].ToString();
					t=checkText(t);
					bool yn=YesNoBox(t);
					return yn;
				case "testit"   : return "0";
				default:
					return null;
			}
		}

		/// <summary>
		/// Converts MP nums in localize text
		/// </summary>
		private static string checkText(string text)
		{
			string conv=text.Trim();
			try 
			{
				int bx=Convert.ToInt32(text);
				if (bx>0)
				{
					conv=GUILocalizeStrings.Get(bx); 
				} 
			} 
			catch(Exception ex) 
			{
			}
			return conv;
		}

		/// <summary>
		/// Shows Yes No Box
		/// </summary>
		private static bool YesNoBox(string text)
		{
			GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
			if (null==dlgYesNo) return false;
			dlgYesNo.SetHeading(""); 
			dlgYesNo.SetLine(1,text);
			dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
			return dlgYesNo.IsConfirmed;
		}
		#endregion

	}
}
