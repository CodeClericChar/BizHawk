﻿using System;
using BizHawk.Common.NumberExtensions;

namespace BizHawk.Emulation.Common.Components.MC6809
{
	public partial class MC6809
	{
		public void Read_Func(ushort dest, ushort src)
		{
			if (CDLCallback != null)
			{
				if (src == PC) CDLCallback(Regs[src], eCDLogMemFlags.FetchOperand);
				else CDLCallback(Regs[src], eCDLogMemFlags.Data);
			}
			Regs[dest] = ReadMemory(Regs[src]);
		}

		public void Read_Inc_Func(ushort dest, ushort src)
		{
			if (CDLCallback != null)
			{
				if (src == PC) CDLCallback(Regs[src], eCDLogMemFlags.FetchOperand);
				else CDLCallback(Regs[src], eCDLogMemFlags.Data);
			}
			//Console.WriteLine(dest + " " + src + " " + opcode_see);

			Regs[dest] = ReadMemory(Regs[src]);

			Regs[src]++;
		}

		public void Write_Func(ushort dest_l, ushort dest_h, ushort src)
		{
			ushort addr = (ushort)(Regs[dest_l] | (Regs[dest_h]) << 8);
			if (CDLCallback != null) CDLCallback(addr, eCDLogMemFlags.Write | eCDLogMemFlags.Data);
			WriteMemory(addr, (byte)Regs[src]);
		}

		public void NEG_8_Func(ushort src)
		{
			int Reg16_d = 0;
			Reg16_d -= Regs[src];

			FlagC = Regs[src] != 0x0;
			FlagZ = (Reg16_d & 0xFF) == 0;
			FlagV = Regs[src] == 0x80;
			FlagN = (Reg16_d & 0xFF) > 127;

			ushort ans = (ushort)(Reg16_d & 0xFF);
			// redo for half carry flag
			Reg16_d = 0;
			Reg16_d -= (Regs[src] & 0xF);
			FlagH = Reg16_d.Bit(4);
			Regs[src] = ans;
		}

		public void Write_Dec_Lo_Func(ushort dest, ushort src)
		{
			if (CDLCallback != null) CDLCallback(Regs[dest], eCDLogMemFlags.Write | eCDLogMemFlags.Data);
			WriteMemory(Regs[dest], (byte)Regs[src]);
			Regs[dest] -= 1;
		}

		public void Write_Dec_HI_Func(ushort dest, ushort src)
		{
			if (CDLCallback != null) CDLCallback(Regs[dest], eCDLogMemFlags.Write | eCDLogMemFlags.Data);
			WriteMemory(Regs[dest], (byte)(Regs[src] >> 8));
			Regs[dest] -= 1;
		}

		public void Write_Hi_Func(ushort dest, ushort src)
		{
			if (CDLCallback != null) CDLCallback(Regs[dest], eCDLogMemFlags.Write | eCDLogMemFlags.Data);
			WriteMemory(Regs[dest], (byte)(Regs[src] >> 8));
		}

		public void Write_Hi_Inc_Func(ushort dest, ushort src)
		{
			if (CDLCallback != null) CDLCallback(Regs[dest], eCDLogMemFlags.Write | eCDLogMemFlags.Data);
			WriteMemory(Regs[dest], (byte)(Regs[src] >> 8));
			Regs[dest]++;
		}

		public void TR_Func(ushort dest, ushort src)
		{
			Regs[dest] = Regs[src];
		}

		public void TST_Func(ushort src)
		{
			FlagZ = Regs[src] == 0;
			FlagV = false;
			FlagN = (Regs[src] & 0xFF) > 127;
		}

		public void CLR_Func(ushort src)
		{
			Regs[src] = 0;

			FlagZ = true;
			FlagV = false;
			FlagC = false;
			FlagN = false;
		}

		// source is considered a 16 bit signed value, used for long relative branch
		// no flags used
		public void ADD16BR_Func(ushort dest, ushort src)
		{
			Regs[dest] = (ushort)(Regs[dest] + (short)Regs[src]);
		}

		public void ADD8BR_Func(ushort dest, ushort src)
		{
			if (Regs[src] > 127) { Regs[src] |= 0xFF00; }
			Regs[dest] = (ushort)(Regs[dest] + (short)Regs[src]);
		}

		public void Mul_Func()
		{
			Regs[ALU] = (ushort)(Regs[A] * Regs[B]);
			D = Regs[ALU];
			FlagC = Regs[A] > 127;
			FlagZ = D == 0;
		}

		public void ADD8_Func(ushort dest, ushort src)
		{
			int Reg16_d = Regs[dest];
			Reg16_d += Regs[src];

			FlagC = Reg16_d.Bit(8);
			FlagZ = (Reg16_d & 0xFF) == 0;

			ushort ans = (ushort)(Reg16_d & 0xFF);

			// redo for half carry flag
			Reg16_d = Regs[dest] & 0xF;
			Reg16_d += (Regs[src] & 0xF);

			FlagH = Reg16_d.Bit(4);
			FlagV = (Regs[dest].Bit(7) != Regs[src].Bit(7)) && (Regs[dest].Bit(7) != ans.Bit(7));
			FlagN = false;

			Regs[dest] = ans;
		}

		public void SUB8_Func(ushort dest, ushort src)
		{
			int Reg16_d = Regs[dest];
			Reg16_d -= Regs[src];

			FlagC = Reg16_d.Bit(8);
			FlagZ = (Reg16_d & 0xFF) == 0;

			ushort ans = (ushort)(Reg16_d & 0xFF);

			// redo for half carry flag
			Reg16_d = Regs[dest] & 0xF;
			Reg16_d -= (Regs[src] & 0xF);

			FlagH = Reg16_d.Bit(4);
			FlagN = ans > 127;
			FlagV = (Regs[dest].Bit(7) != Regs[src].Bit(7)) && (Regs[dest].Bit(7) != ans.Bit(7));

			Regs[dest] = ans;
		}

		// same as SUB8 but result not stored
		public void CMP8_Func(ushort dest, ushort src)
		{
			int Reg16_d = Regs[dest];
			Reg16_d -= Regs[src];

			FlagC = Reg16_d.Bit(8);
			FlagZ = (Reg16_d & 0xFF) == 0;

			ushort ans = (ushort)(Reg16_d & 0xFF);

			// redo for half carry flag
			Reg16_d = Regs[dest] & 0xF;
			Reg16_d -= (Regs[src] & 0xF);

			FlagH = Reg16_d.Bit(4);
			FlagN = ans > 127;
			FlagV = (Regs[dest].Bit(7) != Regs[src].Bit(7)) && (Regs[dest].Bit(7) != ans.Bit(7));
		}

		public void BIT_Func(ushort dest, ushort src)
		{
			ushort ans = (ushort)(Regs[dest] & Regs[src]);

			FlagZ = ans == 0;
			FlagV = false;
			FlagN = ans > 127;
		}

		public void ASL_Func(ushort src)
		{
			FlagC = Regs[src].Bit(7);
			FlagV = Regs[src].Bit(7) ^ Regs[src].Bit(6);

			Regs[src] = (ushort)((Regs[src] << 1) & 0xFF);

			FlagZ = Regs[src] == 0;
			FlagH = false;
			FlagN = (Regs[src] & 0xFF) > 127;

		}

		public void ASR_Func(ushort src)
		{
			FlagC = Regs[src].Bit(0);

			ushort temp = (ushort)(Regs[src] & 0x80); // MSB doesn't change in this operation

			Regs[src] = (ushort)((Regs[src] >> 1) | temp);

			FlagZ = Regs[src] == 0;
			FlagH = false;
			FlagN = (Regs[src] & 0xFF) > 127;
		}

		public void LSR_Func(ushort src)
		{
			FlagC = Regs[src].Bit(0);

			Regs[src] = (ushort)(Regs[src] >> 1);

			FlagZ = Regs[src] == 0;
			FlagN = false;
		}

		public void COM_Func(ushort src)
		{
			Regs[src] = (ushort)((~Regs[src]) & 0xFF);

			FlagC = true;
			FlagZ = Regs[src] == 0;
			FlagV = false;
			FlagN = (Regs[src] & 0xFF) > 127;
		}

		public void SEX_Func(ushort src)
		{
			if (Regs[B] > 127)
			{
				Regs[A] = 0xFF;
			}
			else
			{
				Regs[A] = 0;
			}

			FlagZ = D == 0;
			FlagN = Regs[A] > 127;
		}

		public void AND8_Func(ushort dest, ushort src)
		{
			Regs[dest] = (ushort)(Regs[dest] & Regs[src]);

			FlagZ = Regs[dest] == 0;
			FlagV = false;
			FlagN = Regs[dest] > 127;
		}

		public void OR8_Func(ushort dest, ushort src)
		{
			Regs[dest] = (ushort)(Regs[dest] | Regs[src]);

			FlagZ = Regs[dest] == 0;
			FlagV = false;
			FlagN = Regs[dest] > 127;
		}

		public void XOR8_Func(ushort dest, ushort src)
		{
			Regs[dest] = (ushort)(Regs[dest] ^ Regs[src]);

			FlagZ = Regs[dest] == 0;
			FlagV = false;
			FlagN = Regs[dest] > 127;
		}

		public void CP8_Func(ushort dest, ushort src)
		{
			int Reg16_d = Regs[dest];
			Reg16_d -= Regs[src];

			FlagC = Reg16_d.Bit(8);
			FlagZ = (Reg16_d & 0xFF) == 0;

			// redo for half carry flag
			Reg16_d = Regs[dest] & 0xF;
			Reg16_d -= (Regs[src] & 0xF);

			FlagH = Reg16_d.Bit(4);

			FlagN = true;
		}

		public void ROR_Func(ushort src)
		{
			ushort c = (ushort)(FlagC ? 0x80 : 0);

			FlagC = Regs[src].Bit(0);

			Regs[src] = (ushort)(c | (Regs[src] >> 1));

			FlagZ = Regs[src] == 0;
			FlagN = (Regs[src] & 0xFF) > 127;
		}

		public void ROL_Func(ushort src)
		{
			ushort c = (ushort)(FlagC ? 1 : 0);
			FlagC = Regs[src].Bit(7);
			FlagV = Regs[src].Bit(7) ^ Regs[src].Bit(6);


			Regs[src] = (ushort)(((Regs[src] << 1) & 0xFF) | c);

			FlagZ = Regs[src] == 0;
			FlagN = (Regs[src] & 0xFF) > 127;
		}

		public void INC8_Func(ushort src)
		{
			FlagV = Regs[src] == 0x7F;

			Regs[src] = (ushort)((Regs[src] + 1) & 0xFF);

			FlagZ = Regs[src] == 0;
			FlagN = (Regs[src] & 0xFF) > 127;
		}

		public void DEC8_Func(ushort src)
		{
			FlagV = Regs[src] == 0x80;

			Regs[src] = (ushort)((Regs[src] - 1) & 0xFF);

			FlagZ = Regs[src] == 0;
			FlagN = (Regs[src] & 0xFF) > 127;
		}

		public void INC16_Func(ushort src)
		{
			Regs[src] += 1;
		}

		public void DEC16_Func(ushort src)
		{
			Regs[src] -= 1;
		}

		public void ADC8_Func(ushort dest, ushort src)
		{
			int Reg16_d = Regs[dest];
			int c = FlagC ? 1 : 0;

			Reg16_d += (Regs[src] + c);

			FlagC = Reg16_d.Bit(8);
			FlagZ = (Reg16_d & 0xFF) == 0;

			ushort ans = (ushort)(Reg16_d & 0xFF);

			// redo for half carry flag
			Reg16_d = Regs[dest] & 0xF;
			Reg16_d += ((Regs[src] & 0xF) + c);

			FlagH = Reg16_d.Bit(4);
			FlagV = (Regs[dest].Bit(7) != Regs[src].Bit(7)) && (Regs[dest].Bit(7) != ans.Bit(7));
			FlagN = false;

			Regs[dest] = ans;
		}

		public void SBC8_Func(ushort dest, ushort src)
		{
			int Reg16_d = Regs[dest];
			int c = FlagC ? 1 : 0;

			Reg16_d -= (Regs[src] + c);

			FlagC = Reg16_d.Bit(8);
			FlagZ = (Reg16_d & 0xFF) == 0;

			ushort ans = (ushort)(Reg16_d & 0xFF);

			// redo for half carry flag
			Reg16_d = Regs[dest] & 0xF;
			Reg16_d -= ((Regs[src] & 0xF) + c);

			FlagH = Reg16_d.Bit(4);
			FlagN = ans > 127;
			FlagV = (Regs[dest].Bit(7) != Regs[src].Bit(7)) && (Regs[dest].Bit(7) != ans.Bit(7));

			Regs[dest] = ans;
		}

		// DA code courtesy of AWJ: http://forums.nesdev.com/viewtopic.php?f=20&t=15944
		public void DA_Func(ushort src)
		{
			byte a = (byte)Regs[src];

			if (!FlagN)
			{  // after an addition, adjust if (half-)carry occurred or if result is out of bounds
				if (FlagC || a > 0x99) { a += 0x60; FlagC = true; }
				if (FlagH || (a & 0x0f) > 0x09) { a += 0x6; }
			}
			else
			{  // after a subtraction, only adjust if (half-)carry occurred
				if (FlagC) { a -= 0x60; }
				if (FlagH) { a -= 0x6; }
			}

			a &= 0xFF;

			Regs[src] = a;

			FlagZ = a == 0; 
			FlagH = false;
		}

		// used for signed operations
		public void ADDS_Func(ushort dest_l, ushort dest_h, ushort src_l, ushort src_h)
		{
			int Reg16_d = Regs[dest_l];
			int Reg16_s = Regs[src_l];

			Reg16_d += Reg16_s;

			ushort temp = 0;

			// since this is signed addition, calculate the high byte carry appropriately
			if (Reg16_s.Bit(7))
			{
				if (((Reg16_d & 0xFF) >= Regs[dest_l]))
				{
					temp = 0xFF;
				}
				else
				{
					temp = 0;
				}
			}
			else
			{
				temp = (ushort)(Reg16_d.Bit(8) ? 1 : 0);
			}

			ushort ans_l = (ushort)(Reg16_d & 0xFF);

			// JR operations do not effect flags
			if (dest_l != PC)
			{
				FlagC = Reg16_d.Bit(8);

				// redo for half carry flag
				Reg16_d = Regs[dest_l] & 0xF;
				Reg16_d += Regs[src_l] & 0xF;

				FlagH = Reg16_d.Bit(4);
				FlagN = false;
				FlagZ = false; 			
			}

			Regs[dest_l] = ans_l;
			Regs[dest_h] += temp;
			Regs[dest_h] &= 0xFF;

		}

		// D register implied
		public void SUB16_Func(ushort src)
		{
			int Reg16_d = D;
			int Reg16_s = Regs[src];

			Reg16_d -= Reg16_s;

			FlagC = Reg16_d.Bit(16);
			FlagZ = (Reg16_d & 0xFFFF) == 0;

			ushort ans = (ushort)(Reg16_d & 0xFFFF);

			FlagN = ans > 0x7FFF;
			FlagV = (D.Bit(15) != Regs[src].Bit(15)) && (D.Bit(15) != ans.Bit(15));

			D = ans;
		}

		// D register implied
		public void ADD16_Func(ushort src)
		{
			int Reg16_d = D;
			int Reg16_s = Regs[src];

			Reg16_d += Reg16_s;

			FlagC = Reg16_d.Bit(16);
			FlagZ = (Reg16_d & 0xFFFF) == 0;

			ushort ans = (ushort)(Reg16_d & 0xFFFF);

			FlagN = ans > 0x7FFF;
			FlagV = (D.Bit(15) != Regs[src].Bit(15)) && (D.Bit(15) != ans.Bit(15));

			D = ans;
		}

		// D register implied
		public void CMP16D_Func(ushort src)
		{
			int Reg16_d = D;
			int Reg16_s = Regs[src];

			Reg16_d -= Reg16_s;

			FlagC = Reg16_d.Bit(16);
			FlagZ = (Reg16_d & 0xFFFF) == 0;

			ushort ans = (ushort)(Reg16_d & 0xFFFF);

			FlagN = ans > 0x7FFF;
			FlagV = (D.Bit(15) != Regs[src].Bit(15)) && (D.Bit(15) != ans.Bit(15));
		}

		public void CMP16_Func(ushort dest, ushort src)
		{
			int Reg16_d = Regs[dest];
			int Reg16_s = Regs[src];

			Reg16_d -= Reg16_s;

			FlagC = Reg16_d.Bit(16);
			FlagZ = (Reg16_d & 0xFFFF) == 0;

			ushort ans = (ushort)(Reg16_d & 0xFFFF);

			FlagN = ans > 0x7FFF;
			FlagV = (Regs[dest].Bit(15) != Regs[src].Bit(15)) && (Regs[dest].Bit(15) != ans.Bit(15));
		}

		public void EXG_Func(ushort sel)
		{
			ushort src = 0;
			ushort dest = 0;
			ushort temp = 0;
			if ((Regs[sel] & 0x8) == 0)
			{
				switch (Regs[sel] & 0xF)
				{
					case 0: src = Dr; break;
					case 1: src = X; break;
					case 2: src = Y; break;
					case 3: src = US; break;
					case 4: src = SP; break;
					case 5: src = PC; break;
					case 6: src = 0xFF; break;
					case 7: src = 0xFF; break;
				}

				switch ((Regs[sel] >> 4) & 0xF)
				{
					case 0: dest = Dr; break;
					case 1: dest = X; break;
					case 2: dest = Y; break;
					case 3: dest = US; break;
					case 4: dest = SP; break;
					case 5: dest = PC; break;
					case 6: dest = 0xFF; break;
					case 7: dest = 0xFF; break;
					default: dest = 0xFF; break;
				}
			}
			else
			{
				switch (Regs[sel] & 0xF)
				{
					case 8: src = A; break;
					case 9: src = B; break;
					case 10: src = CC; break;
					case 11: src = DP; break;
					case 12: src = 0xFF; break;
					case 13: src = 0xFF; break;
					case 14: src = 0xFF; break;
					case 15: src = 0xFF; break;
				}

				switch ((Regs[sel] >> 4) & 0xF)
				{
					case 8: dest = A; break;
					case 9: dest = B; break;
					case 10: dest = CC; break;
					case 11: dest = DP; break;
					case 12: dest = 0xFF; break;
					case 13: dest = 0xFF; break;
					case 14: dest = 0xFF; break;
					case 15: dest = 0xFF; break;
					default: dest = 0xFF; break;
				}
			}

			if ((src != 0xFF) && (dest != 0xFF))
			{
				if (src == Dr)
				{
					temp = D;
					D = Regs[dest];
					Regs[dest] = temp;
				}
				else if (dest == Dr)
				{
					temp = D;
					D = Regs[src];
					Regs[src] = temp;
				}
				else
				{
					temp = Regs[src];
					Regs[src] = Regs[dest];
					Regs[dest] = temp;
				}
			}
		}

		public void TFR_Func(ushort sel)
		{
			ushort src = 0;
			ushort dest = 0;

			if ((Regs[sel] & 0x8) == 0)
			{
				switch (Regs[sel] & 0xF)
				{
					case 0: dest = Dr; break;
					case 1: dest = X; break;
					case 2: dest = Y; break;
					case 3: dest = US; break;
					case 4: dest = SP; break;
					case 5: dest = PC; break;
					case 6: dest = 0xFF; break;
					case 7: dest = 0xFF; break;
				}

				switch ((Regs[sel] >> 4) & 0xF)
				{
					case 0: src = Dr; break;
					case 1: src = X; break;
					case 2: src = Y; break;
					case 3: src = US; break;
					case 4: src = SP; break;
					case 5: src = PC; break;
					case 6: src = 0xFF; break;
					case 7: src = 0xFF; break;
					default: src = 0xFF; break;
				}
			}
			else
			{
				switch (Regs[sel] & 0xF)
				{
					case 8: dest = A; break;
					case 9: dest = B; break;
					case 10: dest = CC; break;
					case 11: dest = DP; break;
					case 12: dest = 0xFF; break;
					case 13: dest = 0xFF; break;
					case 14: dest = 0xFF; break;
					case 15: dest = 0xFF; break;
				}

				switch ((Regs[sel] >> 4) & 0xF)
				{
					case 8: src = A; break;
					case 9: src = B; break;
					case 10: src = CC; break;
					case 11: src = DP; break;
					case 12: src = 0xFF; break;
					case 13: src = 0xFF; break;
					case 14: src = 0xFF; break;
					case 15: src = 0xFF; break;
					default: src = 0xFF; break;
				}
			}

			if ((src != 0xFF) && (dest != 0xFF))
			{
				if (src == Dr)
				{
					Regs[dest] = D;
				}
				else if (dest == Dr)
				{
					D = Regs[src];
				}
				else
				{
					Regs[dest] = Regs[src];
				}
			}
		}
	}
}
