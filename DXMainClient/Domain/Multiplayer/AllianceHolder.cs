using ClientCore;
using ClientCore.Enums;

using Rampastring.Tools;
using System.Collections.Generic;

namespace DTAClient.Domain.Multiplayer
{
    /// <summary>
    /// A helper class for setting up alliances in spawn.ini.
    /// Supports all teams defined in ProgramConstants.TEAMS (A–H = TeamId 1–8).
    /// </summary>
    public static class AllianceHolder
    {
        public static void WriteInfoToSpawnIni(
            List<PlayerInfo> players,
            List<PlayerInfo> aiPlayers,
            List<int> multiCmbIndexes,
            List<PlayerHouseInfo> playerHouseInfos,
            List<TeamStartMapping> teamStartMappings,
            IniFile spawnIni
        )
        {
            // TeamId is 1-based (A=1 … H=8). Index 0 unused.
            int teamCount = ProgramConstants.TEAMS.Count;
            var teamMembers = new List<int>[teamCount + 1];
            for (int t = 1; t <= teamCount; t++)
                teamMembers[t] = new List<int>();

            // Human players
            for (int pId = 0; pId < players.Count; pId++)
            {
                var phi = playerHouseInfos[pId];
                int teamId = players[pId].TeamId;
                if (teamId <= 0)
                    teamId = teamStartMappings?.Find(sa => sa.StartingWaypoint == phi.StartingWaypoint)?.TeamId ?? 0;

                if (teamId > 0 && teamId <= teamCount)
                    teamMembers[teamId].Add(multiCmbIndexes.FindIndex(c => c == pId) + 1);
            }

            // AI players
            int multiId = multiCmbIndexes.Count + 1;
            for (int aiId = 0; aiId < aiPlayers.Count; aiId++)
            {
                var phi = playerHouseInfos[multiCmbIndexes.Count + aiId];
                int teamId = aiPlayers[aiId].TeamId;
                if (teamId <= 0)
                    teamId = teamStartMappings?.Find(sa => sa.StartingWaypoint == phi.StartingWaypoint)?.TeamId ?? 0;

                if (teamId > 0 && teamId <= teamCount)
                    teamMembers[teamId].Add(multiId);

                multiId++;
            }

            for (int t = 1; t <= teamCount; t++)
                WriteAlliances(teamMembers[t], spawnIni);
        }

        private static void WriteAlliances(List<int> teamHouseMemberIds, IniFile spawnIni)
        {
            foreach (int houseId in teamHouseMemberIds)
            {
                bool selfFound = false;

                for (int allyId = 0; allyId < teamHouseMemberIds.Count; allyId++)
                {
                    int allyHouseId = teamHouseMemberIds[allyId];

                    if (allyHouseId == houseId)
                    {
                        selfFound = true;
                    }
                    else
                    {
                        spawnIni.SetIntValue("Multi" + houseId + "_Alliances",
                                             "HouseAlly" + GetHouseAllyIndexString(allyId, selfFound),
                                             ClientConfiguration.Instance.ClientGameType == ClientType.RA
                                             ? allyHouseId + 11  // RA multiplayer house IDs shifted +12 (from -1 to +11)
                        : allyHouseId - 1);
                    }
                }
            }
        }

        private static string GetHouseAllyIndexString(int allyId, bool selfFound)
        {
            if (selfFound)
                allyId = allyId - 1;

            switch (allyId)
            {
                case 0: return "One";
                case 1: return "Two";
                case 2: return "Three";
                case 3: return "Four";
                case 4: return "Five";
                case 5: return "Six";
                case 6: return "Seven";
            }

            return "None" + allyId;
        }
    }
}
