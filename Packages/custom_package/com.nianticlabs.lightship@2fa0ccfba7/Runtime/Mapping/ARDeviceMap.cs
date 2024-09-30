// Copyright 2022-2024 Niantic.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Niantic.Lightship.AR.LocationAR;
using Niantic.Lightship.AR.PersistentAnchors;
using Niantic.Lightship.AR.Utilities;
using UnityEngine;

namespace Niantic.Lightship.AR.Mapping
{
    /// <summary>
    /// ARDeviceMap encapsulates device map data generated from mapping process, and provides to serialize/deserialize
    /// device map for persistent or sharing purpose.
    /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
    /// </summary>
    [Experimental]
    public class ARDeviceMap
    {
        [Serializable]
        [Experimental]
        public struct SerializeableDeviceMapNode
        {
            public ulong _subId1;
            public ulong _subId2;
            public byte[] _mapData;
            public byte[] _anchorPayload;
        }

        [Serializable]
        [Experimental]
        public struct SerializableDeviceMap
        {
            public SerializeableDeviceMapNode[] _serializeableSingleDeviceMaps;

            // TODO: add graphs

            // TODO: add default anchor index

            // TODO: add thumbnail image
        }


        /// <summary>
        /// Get a list of SerializeableDeviceMapNode, either mapped on the device or deserialized
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        [Experimental]
        public List<SerializeableDeviceMapNode> DeviceMapNodes
        {
            get => _deviceMapNodes;
        }

        public ARDeviceMap()
        {

        }

        protected List<SerializeableDeviceMapNode> _deviceMapNodes = new ();
        protected int _defaultAnchorIndex = 0;

        /// <summary>
        /// Add a device map node. This method is meant to be called by ARDeviceMappingManager when a device map is generated.
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        /// <param name="subId1"></param>
        /// <param name="subId2"></param>
        /// <param name="mapData"></param>
        /// <param name="anchorPayload"></param>
        [Experimental]
        public void AddDeviceMapNode(ulong subId1, ulong subId2, byte[] mapData, byte[] anchorPayload)
        {
            if (!DeviceMapFeatureFlag.IsFeatureEnabled())
            {
                return;
            }

            var mapNode = new SerializeableDeviceMapNode
            {
                _subId1 = subId1,
                _subId2 = subId2,
                _mapData = mapData,
                _anchorPayload = anchorPayload
            };
            _deviceMapNodes.Add(mapNode);
        }

        /// <summary>
        /// Get serialized device map
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        /// <returns>Serailized device map as byte array</returns>
        [Experimental]
        public byte[] SerializeDeviceMap()
        {
            if (!DeviceMapFeatureFlag.IsFeatureEnabled())
            {
                return null;
            }

            // construct serializable DeviceMap
            var serialzableDeviceMapNodes = new SerializeableDeviceMapNode[_deviceMapNodes.Count];
            for (var i = 0; i < _deviceMapNodes.Count; i++)
            {
                serialzableDeviceMapNodes[i] = _deviceMapNodes[i];
            }

            var serialiableMapNode = new SerializableDeviceMap()
            {
                _serializeableSingleDeviceMaps = serialzableDeviceMapNodes
            };

            //
            byte[] buf;
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, serialiableMapNode);
                buf = stream.ToArray();
            }

            return buf;
        }

        /// <summary>
        /// Create an instance of ARDeviceMap from serialized device map data
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        /// <param name="serializedDeviceMap">Serialized device map as byte array</param>
        /// <returns>An instance of ARDeviceMap</returns>
        [Experimental]
        public static ARDeviceMap CreateARDeviceMapFromSerializedData(byte[] serializedDeviceMap)
        {
            if (!DeviceMapFeatureFlag.IsFeatureEnabled())
            {
                return null;
            }

            SerializableDeviceMap serialiableMapNode;
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream(serializedDeviceMap))
            {
                serialiableMapNode = (SerializableDeviceMap)formatter.Deserialize(stream);
            }

            var deviceMap = new ARDeviceMap();
            for (var i = 0; i < serialiableMapNode._serializeableSingleDeviceMaps.Length; i++)
            {
                deviceMap._deviceMapNodes.Add(serialiableMapNode._serializeableSingleDeviceMaps[i]) ;
            }
            return deviceMap;
        }

        /// <summary>
        /// Get the default anchor payload of this device map
        /// @note This is an experimental feature, and is subject to breaking changes or deprecation without notice
        /// </summary>
        /// <returns>Persistent anchor payload as byte array</returns>
        [Experimental]
        public byte[] GetDefaultAnchorPayload()
        {
            if (!DeviceMapFeatureFlag.IsFeatureEnabled())
            {
                return null;
            }

            if (_deviceMapNodes.Count > 0)
            {
                return _deviceMapNodes[_defaultAnchorIndex]._anchorPayload;
            }
            return null;
        }
    }
}
