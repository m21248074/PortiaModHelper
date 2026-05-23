using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RelicManager
{
	// Token: 0x02000002 RID: 2
	public class Relics
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public static IList<Relic> RelicsList { get; } = new ReadOnlyCollection<Relic>(new List<Relic>
		{
			new Relic("AI Model", 5, 3, 270.0, "Desert Abandoned Ruins", new int[]
			{
				3000173
			}, new int[]
			{
				4010110,
				4010111,
				4010112,
				4010113,
				4010114
			}),
			new Relic("Ancient Computer", 2, 1, 180.0, "Somber Marsh Abandoned Ruins", new int[]
			{
				3000240
			}, new int[]
			{
				4010146,
				4010147
			}),
			new Relic("Autumn Doll", 2, 1, 180.0, "Desert Abandoned Ruins", new int[]
			{
				3000168
			}, new int[]
			{
				4010098,
				4010099
			}),
			new Relic("Bonsai", 4, 2, 240.0, "Desert Abandoned Ruins", new int[]
			{
				3000114
			}, new int[]
			{
				4010137,
				4010138,
				4010139,
				4010140
			}),
			new Relic("Catmaid Statue", 4, 2, 240.0, "Abandoned Ruins #2", new int[]
			{
				3000116
			}, new int[]
			{
				4010080,
				4010081,
				4010082,
				4010083
			}),
			new Relic("Cauldron", 4, 2, 240.0, "Somber Marsh Abandoned Ruins", new int[]
			{
				3000279
			}, new int[]
			{
				4010164,
				4010165,
				4010166,
				4010167
			}),
			new Relic("Clay Figure: Female", 3, 1, 210.0, "Desert Abandoned Ruins", new int[]
			{
				3000175
			}, new int[]
			{
				4010118,
				4010119,
				4010120
			}),
			new Relic("Clay Figure: Male", 3, 1, 210.0, "Desert Abandoned Ruins", new int[]
			{
				3000174
			}, new int[]
			{
				4010115,
				4010116,
				4010117
			}),
			new Relic("Clear Sky Doll", 3, 1, 210.0, "Somber Marsh Abandoned Ruins", new int[]
			{
				3000278
			}, new int[]
			{
				4010161,
				4010162,
				4010163
			}),
			new Relic("Damour Roly-Poly", 3, 1, 210.0, "Somber Marsh Abandoned Ruins", new int[]
			{
				3000277
			}, new int[]
			{
				4010158,
				4010159,
				4010160
			}),
			new Relic("Diving Helmet", 3, 1, 210.0, "Somber Marsh Abandoned Ruins", new int[]
			{
				7000111
			}, new int[]
			{
				4010141,
				4010142,
				4010143
			}),
			new Relic("Duck on a King", 3, 1, 210.0, "Abandoned Ruins #2", new int[]
			{
				3000109
			}, new int[]
			{
				4010086,
				4010087,
				4010088
			}),
			new Relic("Fan Model", 3, 1, 210.0, "Desert Abandoned Ruins", new int[]
			{
				3000180
			}, new int[]
			{
				4010134,
				4010135,
				4010136
			}),
			new Relic("Fish Sub", 3, 1, 210.0, "Desert Abandoned Ruins", new int[]
			{
				3000108
			}, new int[]
			{
				4010051,
				4010052,
				4010053
			}),
			new Relic("Fortune Cat", 4, 2, 240.0, "Desert Abandoned Ruins", new int[]
			{
				3000172
			}, new int[]
			{
				4010106,
				4010107,
				4010108,
				4010109
			}),
			new Relic("French Horn", 2, 1, 180.0, "Somber Marsh Abandoned Ruins", new int[]
			{
				7000115
			}, new int[]
			{
				4010144,
				4010145
			}),
			new Relic("Galloping Horse", 4, 2, 240.0, "Abandoned Ruins #1", new int[]
			{
				3000100
			}, new int[]
			{
				4010062,
				4010063,
				4010064,
				4010065
			}),
			new Relic("Goddess Statue", 5, 2, 240.0, "Abandoned Ruins #2", new int[]
			{
				3000098
			}, new int[]
			{
				4010089,
				4010090,
				4010091,
				4010092,
				4010093
			}),
			new Relic("Incredible Mandy", 2, 1, 180.0, "Somber Marsh Abandoned Ruins", new int[]
			{
				3000315
			}, new int[]
			{
				4010168,
				4010169
			}),
			new Relic("Joystick", 2, 1, 180.0, "Abandoned Ruins #1", new int[]
			{
				7000040
			}, new int[]
			{
				4010074,
				4010075
			}),
			new Relic("Magic Lamp", 3, 1, 210.0, "Abandoned Ruins #2", new int[]
			{
				3000107
			}, new int[]
			{
				4010066,
				4010067,
				4010068
			}),
			new Relic("Monster Toy", 2, 1, 180.0, "Abandoned Ruins #1", new int[]
			{
				3000106
			}, new int[]
			{
				4010084,
				4010085
			}),
			new Relic("Monument Model", 4, 2, 240.0, "Abandoned Ruins #1", new int[]
			{
				3000115
			}, new int[]
			{
				4010021,
				4010022,
				4010023,
				4010024
			}),
			new Relic("Old Talker", 3, 1, 210.0, "Abandoned Ruins #1", new int[]
			{
				3000110
			}, new int[]
			{
				4010054,
				4010055,
				4010056
			}),
			new Relic("Old Thermos", 2, 1, 180.0, "Abandoned Ruins #1", new int[]
			{
				3000099
			}, new int[]
			{
				4010057,
				4010058
			}),
			new Relic("Owl Clock", 4, 1, 210.0, "Abandoned Ruins #2", new int[]
			{
				3000111
			}, new int[]
			{
				4010011,
				4010012,
				4010013,
				4010014
			}),
			new Relic("Performance Center Model", 4, 2, 240.0, "Abandoned Ruins #2", new int[]
			{
				3000133
			}, new int[]
			{
				4010076,
				4010077,
				4010078,
				4010079
			}),
			new Relic("Plane Model", 4, 2, 240.0, "Desert Abandoned Ruins", new int[]
			{
				3000177
			}, new int[]
			{
				4010124,
				4010125,
				4010126,
				4010127
			}),
			new Relic("Porcelain Waterholder", 2, 1, 180.0, "Abandoned Ruins #1", new int[]
			{
				3000112
			}, new int[]
			{
				4010045,
				4010046
			}),
			new Relic("Power Lamp", 3, 1, 210.0, "Abandoned Ruins #2", new int[]
			{
				3000125
			}, new int[]
			{
				4010069,
				4010070,
				4010071
			}),
			new Relic("Rocket Model", 3, 1, 210.0, "Desert Abandoned Ruins", new int[]
			{
				3000176
			}, new int[]
			{
				4010121,
				4010122,
				4010123
			}),
			new Relic("Sailboat Model", 3, 1, 210.0, "Desert Abandoned Ruins", new int[]
			{
				3000178
			}, new int[]
			{
				4010128,
				4010129,
				4010130
			}),
			new Relic("Sculpted Lion", 4, 2, 240.0, "Desert Abandoned Ruins", new int[]
			{
				3000171
			}, new int[]
			{
				4010102,
				4010103,
				4010104,
				4010105
			}),
			new Relic("Soldier with Axe", 5, 3, 270.0, "Abandoned Ruins #2", new int[]
			{
				3000102
			}, new int[]
			{
				4010030,
				4010031,
				4010032,
				4010033,
				4010034
			}),
			new Relic("Soldier with Blade", 5, 3, 270.0, "Abandoned Ruins #2", new int[]
			{
				3000103
			}, new int[]
			{
				4010035,
				4010036,
				4010037,
				4010038,
				4010039
			}),
			new Relic("Soldier with Lance", 5, 2, 270.0, "Abandoned Ruins #2", new int[]
			{
				3000104
			}, new int[]
			{
				4010040,
				4010041,
				4010042,
				4010043,
				4010044
			}),
			new Relic("Soldier with Scepter", 5, 3, 270.0, "Abandoned Ruins #2", new int[]
			{
				3000101
			}, new int[]
			{
				4010025,
				4010026,
				4010027,
				4010028,
				4010029
			}),
			new Relic("Sphere Trophy", 3, 1, 210.0, "Desert Abandoned Ruins", new int[]
			{
				3000179
			}, new int[]
			{
				4010131,
				4010132,
				4010133
			}),
			new Relic("Spring Doll", 2, 1, 180.0, "Desert Abandoned Ruins", new int[]
			{
				3000166
			}, new int[]
			{
				4010094,
				4010095
			}),
			new Relic("Summer Doll", 2, 1, 180.0, "Desert Abandoned Ruins", new int[]
			{
				3000167
			}, new int[]
			{
				4010096,
				4010097
			}),
			new Relic("Sunny Side", 2, 1, 180.0, "Abandoned Ruins #1", new int[]
			{
				3000132
			}, new int[]
			{
				4010072,
				4010073
			}),
			new Relic("The Thinking Can", 4, 2, 240.0, "Abandoned Ruins #1", new int[]
			{
				3000105
			}, new int[]
			{
				4010047,
				4010048,
				4010049,
				4010050
			}),
			new Relic("Typewriter", 2, 1, 180.0, "Somber Marsh Abandoned Ruins", new int[]
			{
				3000241
			}, new int[]
			{
				4010148,
				4010149
			}),
			new Relic("Vinyl Record Player", 3, 1, 210.0, "Somber Marsh Abandoned Ruins", new int[]
			{
				3000242
			}, new int[]
			{
				4010150,
				4010151,
				4010152
			}),
			new Relic("Weird Glass Jar", 3, 1, 210.0, "Abandoned Ruins #1", new int[]
			{
				3000113
			}, new int[]
			{
				4010059,
				4010060,
				4010061
			}),
			new Relic("Welding Helmet", 2, 1, 180.0, "Somber Marsh Abandoned Ruins", new int[]
			{
				1021017,
				1101805,
				1101806,
				1101807,
				1101808,
				1101809,
				1101813,
				1101816,
				1101819,
				7000112
			}, new int[]
			{
				4010153,
				4010154
			}),
			new Relic("Winter Doll", 2, 1, 180.0, "Desert Abandoned Ruins", new int[]
			{
				3000169
			}, new int[]
			{
				4010100,
				4010101
			})
		});

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000002 RID: 2 RVA: 0x00002057 File Offset: 0x00000257
		public static int DataDiscID { get; } = 2060001;
	}
}
