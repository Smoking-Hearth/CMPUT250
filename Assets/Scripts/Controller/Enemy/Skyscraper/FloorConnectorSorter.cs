using UnityEngine;
using System.Collections.Generic;

public class FloorConnectorSorter : MonoBehaviour
{
    private Dictionary<ConnectorFloor.Connections, List<ConnectorFloor>> connectors;

    public void Sort(ConnectorFloor[] unsorted)
    {
        connectors = new Dictionary<ConnectorFloor.Connections, List<ConnectorFloor>>();
        for (int i = 0; i < unsorted.Length; i++)
        {
            if (!connectors.ContainsKey(unsorted[i].floorConnections))
            {
                connectors.Add(unsorted[i].floorConnections, new List<ConnectorFloor>());
            }
            connectors[unsorted[i].floorConnections].Add(unsorted[i]);
        }
    }

    public ConnectorFloor GetFittingConnector(ConnectorFloor.Connections required, ConnectorFloor.Connections exclude)
    {
        List<ConnectorFloor.Connections> validKeys = new List<ConnectorFloor.Connections>();
        foreach (ConnectorFloor.Connections key in connectors.Keys)
        {
            if ((required & ~key) == 0)
            {
                if ((key & ConnectorFloor.Connections.DOWN) > 0 && (required & ConnectorFloor.Connections.DOWN) == 0)
                {
                    continue;
                }

                if ((key & exclude) > 0)
                {
                    continue;
                }
                validKeys.Add(key);
            }
        }

        if (validKeys.Count == 0)
        {
            return null;
        }

        int keyIndex = Random.Range(0, validKeys.Count);
        List<ConnectorFloor> connectorList = connectors[validKeys[keyIndex]];

        int randomIndex = Random.Range(0, connectorList.Count);
        return connectorList[randomIndex];
    }
}
