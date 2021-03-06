/**
 * Copyright (c) 2019 Emilian Roman
 * Copyright (c) 2020 Noah Sherwin
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static System.IO.SearchOption;
using static HXE.Console;
using static HXE.HCE.Profile.ProfileAudio;
using static HXE.HCE.Profile.ProfileDetails;
using static HXE.HCE.Profile.ProfileNetwork;
using static HXE.HCE.Profile.ProfileVideo;
using static HXE.HCE.Profile.ProfileInput;
using static HXE.Paths;
using Directory = System.IO.Directory;

namespace HXE.HCE
{
  /// <inheritdoc />
  /// <summary>
  ///   Object representing a HCE profile blam.sav binary.
  /// </summary>
  /// <see cref="https://c20.reclaimers.net/h1/engine/files/#blam-sav"/>
  public class Profile : File
  {
    public ProfileDetails  Details { get; set; } = new ProfileDetails();   /* profile name & online player colour */
    public ProfileMouse    Mouse   { get; set; } = new ProfileMouse();     /* sensitivities & vertical axis inversion */
    public ProfileGamepad Gamepad0 { get; set; } = new ProfileGamepad();  /* sensitivities & vertical axis inversion */
    public ProfileGamepad Gamepad1 { get; set; } = new ProfileGamepad();  /* sensitivities & vertical axis inversion */
    public ProfileGamepad Gamepad2 { get; set; } = new ProfileGamepad();  /* sensitivities & vertical axis inversion */
    public ProfileGamepad Gamepad3 { get; set; } = new ProfileGamepad();  /* sensitivities & vertical axis inversion */
    public ProfileAudio    Audio   { get; set; } = new ProfileAudio();     /* volumes, qualities, varieties & eax/hw */
    public ProfileVideo    Video   { get; set; } = new ProfileVideo();     /* resolutions, rates, effects & qualities */
    public ProfileNetwork  Network { get; set; } = new ProfileNetwork();   /* connection types & server/client ports */
    public ProfileInput    Input   { get; set; } = new ProfileInput();     /* input-action mapping */

    /// <summary>
    ///   Saves object state to the inbound file.
    /// </summary>
    public void Save()
    {
      /**
       * We first open up a binary writer for storing the configuration in the blam.sav binary. Data is all written in
       * one go to the binary on the filesystem.
       */

      using (var fs = new FileStream(Path, FileMode.Open, FileAccess.ReadWrite))
      using (var ms = new MemoryStream(8192))
      using (var bw = new BinaryWriter(ms))
      {
        void WriteBoolean(Offset offset, bool data)
        {
          ms.Position = (int) offset;
          bw.Write(data);
        }

        void WriteUShort(Offset offset, ushort data)
        {
          ms.Position = (int) offset;
          bw.Write(data);
        }

        void WriteByte(Offset offset, byte data)
        {
          ms.Position = (int) offset;
          bw.Write(data);
        }

        fs.Position = 0;
        fs.CopyTo(ms);

        /**
         * The name is stored in UTF-16; hence, we rely on the Unicode class to encode the string to a byte array for
         * writing the profile name in the binary.
         */

        ms.Position = (int) Offset.ProfileName;
        bw.Write(Encoding.Unicode.GetBytes(Details.Name));

        /**
         * First, we'll take care of the enum options. Storing them is rather straightforward: we cast their values to
         * 16-bit integers, which can be then written to the binary.
         */

        WriteUShort(Offset.ProfileColour,         (ushort) Details.Colour);
        WriteUShort(Offset.VideoFrameRate,        (ushort) Video.FrameRate);
        WriteUShort(Offset.VideoQualityParticles, (ushort) Video.Particles);
        WriteUShort(Offset.VideoQualityTextures,  (ushort) Video.Quality);
        WriteUShort(Offset.AudioQuality,          (ushort) Audio.Quality);
        WriteUShort(Offset.AudioVariety,          (ushort) Audio.Variety);
        WriteUShort(Offset.NetworkConnectionType, (ushort) Network.Connection);

        /**
         * The following values are values which can have any integer (within the limits of the data types, of course).
         */

        WriteUShort(Offset.VideoResolutionWidth,  Video.Resolution.Width);
        WriteUShort(Offset.VideoResolutionHeight, Video.Resolution.Height);
        WriteUShort(Offset.NetworkPortServer,     Network.Port.Server);
        WriteUShort(Offset.NetworkPortClient,     Network.Port.Client);

        WriteByte(Offset.VideoRefreshRate,           Video.RefreshRate);
        WriteByte(Offset.VideoMiscellaneousGamma,    Video.Gamma);
        WriteByte(Offset.MouseSensitivityHorizontal, Mouse.Sensitivity.Horizontal);
        WriteByte(Offset.MouseSensitivityVertical,   Mouse.Sensitivity.Vertical);
        WriteByte(Offset.GP0SensitivityHorizontal,   Gamepad0.Sensitivity.Horizontal);
        WriteByte(Offset.GP1SensitivityHorizontal,   Gamepad1.Sensitivity.Horizontal);
        WriteByte(Offset.GP2SensitivityHorizontal,   Gamepad2.Sensitivity.Horizontal);
        WriteByte(Offset.GP3SensitivityHorizontal,   Gamepad3.Sensitivity.Horizontal);
        WriteByte(Offset.GP0SensitivityVertical,     Gamepad0.Sensitivity.Vertical);
        WriteByte(Offset.GP1SensitivityVertical,     Gamepad1.Sensitivity.Vertical);
        WriteByte(Offset.GP2SensitivityVertical,     Gamepad2.Sensitivity.Vertical);
        WriteByte(Offset.GP3SensitivityVertical,     Gamepad3.Sensitivity.Vertical);
        WriteByte(Offset.AudioVolumeMaster,          Audio.Volume.Master);
        WriteByte(Offset.AudioVolumeEffects,         Audio.Volume.Effects);
        WriteByte(Offset.AudioVolumeMusic,           Audio.Volume.Music);

        /**
         * As for the boolean values, we convert them behind the scene to their integer equivalents -- 1 and 0 for true
         * and false, respectively.
         */

        WriteBoolean(Offset.MouseInvertVerticalAxis, Mouse.InvertVerticalAxis);
        WriteBoolean(Offset.VideoEffectsSpecular,    Video.Effects.Specular);
        WriteBoolean(Offset.VideoEffectsShadows,     Video.Effects.Shadows);
        WriteBoolean(Offset.VideoEffectsDecals,      Video.Effects.Decals);
        WriteBoolean(Offset.AudioEAX,                Audio.EAX);
        WriteBoolean(Offset.AudioHWA,                Audio.HWA);

        /**
         * Mapping is conducted by writing values at offsets, where values = actions and offsets = inputs.
         */

        /** Erase Old Input Bindings */
        {
          foreach (var offset in Enum.GetValues(typeof(Keyboard)))
          {
            Debug("Nulling input - " + offset);

            ms.Position = (int) offset;
            bw.Write((ushort) 0x7FFF);
          }

          foreach (var offset in Enum.GetValues(typeof(Mouse)))
          {
            Debug("Nulling input - " + offset);

            ms.Position = (int) offset;
            bw.Write((ushort) 0x7FFF);
          }

          foreach (var offset in Enum.GetValues(typeof(GP0_Input)))
          {
            Debug("Nulling input - " + offset);

            ms.Position = (int) offset;
            bw.Write((ushort) 0x7FFF);
          }

          foreach (var offset in Enum.GetValues(typeof(GP1_Input)))
          {
            Debug("Nulling input - " + offset);

            ms.Position = (int) offset;
            bw.Write((ushort) 0x7FFF);
          }

          foreach (var offset in Enum.GetValues(typeof(GP2_Input)))
          {
            Debug("Nulling input - " + offset);

            ms.Position = (int) offset;
            bw.Write((ushort) 0x7FFF);
          }

          foreach (var offset in Enum.GetValues(typeof(GP3_Input)))
          {
            Debug("Nulling input - " + offset);

            ms.Position = (int) offset;
            bw.Write((ushort) 0x7FFF);
          }

          foreach (var offset in Enum.GetValues(typeof(GamePadMenu)))
          {
            Debug("Nulling input - " + offset);

            ms.Position = (int) offset;
            bw.Write((ushort) 0xFFFF);
          }
        }

        /** Write New Input Bindings*/
        {
          foreach (var mapping in Input.KeyboardMapping)
          {
            var offset = (int) mapping.Value;  /* button */
            var value  = (ushort) mapping.Key; /* action */

            Debug("Assigning action to input - " + mapping.Key + " -> " + mapping.Value);

            ms.Position = offset;
            bw.Write(value);
          }

          foreach (var mapping in Input.MouseMapping)
          {
            var offset = (int) mapping.Value;  /* button */
            var value  = (ushort) mapping.Key; /* action */

            Debug("Assigning action to input - " + mapping.Key + " -> " + mapping.Value);

            ms.Position = offset;
            bw.Write(value);
          }

          if (Input.GP0_Mapping.Count != 0)
            foreach (var mapping in Input.GP0_Mapping)
            {
              var offset = (int) mapping.Value;  /* button */
              var value  = (ushort) mapping.Key; /* action */

              Debug("Assigning action to input - " + mapping.Key + " -> " + mapping.Value);

              ms.Position = offset;
              bw.Write(value);
            }

          if (Input.GP1_Mapping.Count != 0)
            foreach (var mapping in Input.GP1_Mapping)
            {
              var offset = (int) mapping.Value;  /* button */
              var value  = (ushort) mapping.Key; /* action */

              Debug("Assigning action to input - " + mapping.Key + " -> " + mapping.Value);

              ms.Position = offset;
              bw.Write(value);
            }

          if (Input.GP2_Mapping.Count != 0)
            foreach (var mapping in Input.GP2_Mapping)
            {
              var offset = (int) mapping.Value;  /* button */
              var value  = (ushort) mapping.Key; /* action */

              Debug("Assigning action to input - " + mapping.Key + " -> " + mapping.Value);

              ms.Position = offset;
              bw.Write(value);
            }

          if (Input.GP3_Mapping.Count != 0)
            foreach (var mapping in Input.GP3_Mapping)
            {
              var offset = (int) mapping.Value;  /* button */
              var value  = (ushort) mapping.Key; /* action */

              Debug("Assigning action to input - " + mapping.Key + " -> " + mapping.Value);

              ms.Position = offset;
              bw.Write(value);
            }

          if (Input.Gamepads_Menu.Count != 0)
            foreach (var pair in Input.Gamepads_Menu)
            {
              /* The exception to the rule. The Key is the offset instead of the value. */
              var offset = (int) pair.Key; /* button */
              var value  = (ushort) pair.Value;  /* action */

              Debug("Assigning action to input - " + pair.Key + " -> " + pair.Value);

              ms.Position = offset;
              bw.Write(value);
            }
        }

        /**
         * The layout of the blam.sav is, in a nutshell:
         *
         * [0x0000 - 0x1005] [0x1FFC - 0x2000]
         *         |                 |
         *         |                 + - hash data (4 bytes)
         *         + ------------------- main data (8188 bytes)
         *
         * By ...
         *
         * 1.   truncating the last four bytes (the hash) from the memory stream; then
         * 2.   calculating the hash for the main data (i.e. remaining bytes); then
         * 3.   appending it to the memory stream (i.e. replacing the old hash) ...
         *
         * ... we can write the contents to filesystem and expect HCE to accept both the data and the new hash.
         */

        Debug("Truncating CRC32 checksum from memory stream");

        ms.SetLength(ms.Length - 4);

        Debug("Calculating new CRC32 checksum");

        var hash = GetHash(ms.ToArray());

        Debug("New CRC32 hash - 0x" + BitConverter.ToString(hash).Replace("-", string.Empty));

        ms.SetLength(ms.Length + 4);
        ms.Position = (int) Offset.BinaryCrc32Hash;
        bw.Write(hash);

        Debug("Clearing contents of the profile filesystem binary");

        fs.SetLength(0);

        Debug("Copying profile data in memory to the binary file");

        ms.Position = 0;
        ms.CopyTo(fs);

        Info("Saved profile data to the binary on the filesystem");

        /**
         * This method returns a forged CRC-32 hash which can be written to the end of the blam.sav binary. This allows
         * the binary to be considered valid by HCE. By forged hash, we refer to the bitwise complement of a CRC-32 hash
         * of the blam.sav data.
         */

        byte[] GetHash(byte[] data)
        {
          /**
           * This look-up table has been generated from the standard 0xEDB88320 polynomial, which results in hashes that
           * HCE deems valid. The aforementioned polynomial is the reversed equivalent of 0x04C11DB7, and is used, well,
           * everywhere!
           */

          var crcTable = new uint[]
          {
            0x00000000, 0x77073096, 0xEE0E612C, 0x990951BA, 0x076DC419, 0x706AF48F, 0xE963A535, 0x9E6495A3, 0x0EDB8832,
            0x79DCB8A4, 0xE0D5E91E, 0x97D2D988, 0x09B64C2B, 0x7EB17CBD, 0xE7B82D07, 0x90BF1D91, 0x1DB71064, 0x6AB020F2,
            0xF3B97148, 0x84BE41DE, 0x1ADAD47D, 0x6DDDE4EB, 0xF4D4B551, 0x83D385C7, 0x136C9856, 0x646BA8C0, 0xFD62F97A,
            0x8A65C9EC, 0x14015C4F, 0x63066CD9, 0xFA0F3D63, 0x8D080DF5, 0x3B6E20C8, 0x4C69105E, 0xD56041E4, 0xA2677172,
            0x3C03E4D1, 0x4B04D447, 0xD20D85FD, 0xA50AB56B, 0x35B5A8FA, 0x42B2986C, 0xDBBBC9D6, 0xACBCF940, 0x32D86CE3,
            0x45DF5C75, 0xDCD60DCF, 0xABD13D59, 0x26D930AC, 0x51DE003A, 0xC8D75180, 0xBFD06116, 0x21B4F4B5, 0x56B3C423,
            0xCFBA9599, 0xB8BDA50F, 0x2802B89E, 0x5F058808, 0xC60CD9B2, 0xB10BE924, 0x2F6F7C87, 0x58684C11, 0xC1611DAB,
            0xB6662D3D, 0x76DC4190, 0x01DB7106, 0x98D220BC, 0xEFD5102A, 0x71B18589, 0x06B6B51F, 0x9FBFE4A5, 0xE8B8D433,
            0x7807C9A2, 0x0F00F934, 0x9609A88E, 0xE10E9818, 0x7F6A0DBB, 0x086D3D2D, 0x91646C97, 0xE6635C01, 0x6B6B51F4,
            0x1C6C6162, 0x856530D8, 0xF262004E, 0x6C0695ED, 0x1B01A57B, 0x8208F4C1, 0xF50FC457, 0x65B0D9C6, 0x12B7E950,
            0x8BBEB8EA, 0xFCB9887C, 0x62DD1DDF, 0x15DA2D49, 0x8CD37CF3, 0xFBD44C65, 0x4DB26158, 0x3AB551CE, 0xA3BC0074,
            0xD4BB30E2, 0x4ADFA541, 0x3DD895D7, 0xA4D1C46D, 0xD3D6F4FB, 0x4369E96A, 0x346ED9FC, 0xAD678846, 0xDA60B8D0,
            0x44042D73, 0x33031DE5, 0xAA0A4C5F, 0xDD0D7CC9, 0x5005713C, 0x270241AA, 0xBE0B1010, 0xC90C2086, 0x5768B525,
            0x206F85B3, 0xB966D409, 0xCE61E49F, 0x5EDEF90E, 0x29D9C998, 0xB0D09822, 0xC7D7A8B4, 0x59B33D17, 0x2EB40D81,
            0xB7BD5C3B, 0xC0BA6CAD, 0xEDB88320, 0x9ABFB3B6, 0x03B6E20C, 0x74B1D29A, 0xEAD54739, 0x9DD277AF, 0x04DB2615,
            0x73DC1683, 0xE3630B12, 0x94643B84, 0x0D6D6A3E, 0x7A6A5AA8, 0xE40ECF0B, 0x9309FF9D, 0x0A00AE27, 0x7D079EB1,
            0xF00F9344, 0x8708A3D2, 0x1E01F268, 0x6906C2FE, 0xF762575D, 0x806567CB, 0x196C3671, 0x6E6B06E7, 0xFED41B76,
            0x89D32BE0, 0x10DA7A5A, 0x67DD4ACC, 0xF9B9DF6F, 0x8EBEEFF9, 0x17B7BE43, 0x60B08ED5, 0xD6D6A3E8, 0xA1D1937E,
            0x38D8C2C4, 0x4FDFF252, 0xD1BB67F1, 0xA6BC5767, 0x3FB506DD, 0x48B2364B, 0xD80D2BDA, 0xAF0A1B4C, 0x36034AF6,
            0x41047A60, 0xDF60EFC3, 0xA867DF55, 0x316E8EEF, 0x4669BE79, 0xCB61B38C, 0xBC66831A, 0x256FD2A0, 0x5268E236,
            0xCC0C7795, 0xBB0B4703, 0x220216B9, 0x5505262F, 0xC5BA3BBE, 0xB2BD0B28, 0x2BB45A92, 0x5CB36A04, 0xC2D7FFA7,
            0xB5D0CF31, 0x2CD99E8B, 0x5BDEAE1D, 0x9B64C2B0, 0xEC63F226, 0x756AA39C, 0x026D930A, 0x9C0906A9, 0xEB0E363F,
            0x72076785, 0x05005713, 0x95BF4A82, 0xE2B87A14, 0x7BB12BAE, 0x0CB61B38, 0x92D28E9B, 0xE5D5BE0D, 0x7CDCEFB7,
            0x0BDBDF21, 0x86D3D2D4, 0xF1D4E242, 0x68DDB3F8, 0x1FDA836E, 0x81BE16CD, 0xF6B9265B, 0x6FB077E1, 0x18B74777,
            0x88085AE6, 0xFF0F6A70, 0x66063BCA, 0x11010B5C, 0x8F659EFF, 0xF862AE69, 0x616BFFD3, 0x166CCF45, 0xA00AE278,
            0xD70DD2EE, 0x4E048354, 0x3903B3C2, 0xA7672661, 0xD06016F7, 0x4969474D, 0x3E6E77DB, 0xAED16A4A, 0xD9D65ADC,
            0x40DF0B66, 0x37D83BF0, 0xA9BCAE53, 0xDEBB9EC5, 0x47B2CF7F, 0x30B5FFE9, 0xBDBDF21C, 0xCABAC28A, 0x53B39330,
            0x24B4A3A6, 0xBAD03605, 0xCDD70693, 0x54DE5729, 0x23D967BF, 0xB3667A2E, 0xC4614AB8, 0x5D681B02, 0x2A6F2B94,
            0xB40BBE37, 0xC30C8EA1, 0x5A05DF1B, 0x2D02EF8D
          };

          /**
           * With the available data, we conduct a basic cyclic redundancy check operation on it, using the
           * aforementioned look-up table. This provides us with the CRC-32 for the main data in the blam.sav binary.
           *
           * However, possibly for obfuscation, HCE stores the complement (bitwise NOT) of the hash. This requires us to
           * flip each bit in the entire hash. Once we've done that, we are left with the desired hash that can be
           * stored in the blam.sav binary.
           */

          var hashData = BitConverter.GetBytes(~data.Aggregate(0xFFFFFFFF, (checksumRegister, currentByte) =>
            crcTable[(checksumRegister & 0xFF) ^ Convert.ToByte(currentByte)] ^ (checksumRegister >> 8)));

          for (var i = 0; i < hashData.Length; i++)
            hashData[i] = (byte) ~hashData[i];

          return hashData;
        }
      }
    }

    /// <summary>
    ///   Loads object state from the inbound file.
    /// </summary>
    public void Load()
    {
      using (var reader = new BinaryReader(System.IO.File.Open(Path, FileMode.Open)))
      {
        bool GetBoolean(Offset offset)
        {
          return GetByte(offset) == 1;
        }

        ushort GetShort(Offset offset)
        {
          reader.BaseStream.Seek((int) offset, SeekOrigin.Begin);
          return reader.ReadUInt16();
        }

        byte GetByte(Offset offset)
        {
          reader.BaseStream.Seek((int) offset, SeekOrigin.Begin);
          return reader.ReadByte();
        }

        byte[] GetBytes(Offset offset, int count)
        {
          reader.BaseStream.Seek((int) offset, SeekOrigin.Begin);
          return reader.ReadBytes(count);
        }

        Details.Name = Encoding.Unicode.GetString(GetBytes(Offset.ProfileName, 22)).TrimEnd('\0');

        Details.Colour               = (ColourOptions) GetByte(Offset.ProfileColour);
        Video.FrameRate              = (VideoFrameRate) GetByte(Offset.VideoFrameRate);
        Video.Particles              = (VideoParticles) GetByte(Offset.VideoQualityParticles);
        Video.Quality                = (VideoQuality) GetByte(Offset.VideoQualityTextures);
        Audio.Variety                = (AudioVariety) GetByte(Offset.AudioVariety);
        Network.Connection           = (NetworkConnection) GetByte(Offset.NetworkConnectionType);
        Audio.Quality                = (AudioQuality) GetByte(Offset.AudioQuality);
        Audio.Variety                = (AudioVariety) GetByte(Offset.AudioVariety);
        Mouse.Sensitivity.Horizontal = GetByte(Offset.MouseSensitivityHorizontal);
        Mouse.Sensitivity.Vertical   = GetByte(Offset.MouseSensitivityVertical);
        Gamepad0.Sensitivity.Horizontal = GetByte(Offset.GP0SensitivityHorizontal);
        Gamepad1.Sensitivity.Horizontal = GetByte(Offset.GP1SensitivityHorizontal);
        Gamepad2.Sensitivity.Horizontal = GetByte(Offset.GP2SensitivityHorizontal);
        Gamepad3.Sensitivity.Horizontal = GetByte(Offset.GP3SensitivityHorizontal);
        Gamepad0.Sensitivity.Vertical = GetByte(Offset.GP0SensitivityVertical);
        Gamepad1.Sensitivity.Vertical = GetByte(Offset.GP1SensitivityVertical);
        Gamepad2.Sensitivity.Vertical = GetByte(Offset.GP2SensitivityVertical);
        Gamepad3.Sensitivity.Vertical = GetByte(Offset.GP3SensitivityVertical);
        Video.Resolution.Width       = GetShort(Offset.VideoResolutionWidth);
        Video.Resolution.Height      = GetShort(Offset.VideoResolutionHeight);
        Video.RefreshRate            = GetByte(Offset.VideoRefreshRate);
        Video.Gamma                  = GetByte(Offset.VideoMiscellaneousGamma);
        Audio.Volume.Master          = GetByte(Offset.AudioVolumeMaster);
        Audio.Volume.Effects         = GetByte(Offset.AudioVolumeEffects);
        Audio.Volume.Music           = GetByte(Offset.AudioVolumeMusic);
        Network.Port.Server          = GetShort(Offset.NetworkPortServer);
        Network.Port.Client          = GetShort(Offset.NetworkPortClient);
        Mouse.InvertVerticalAxis     = GetBoolean(Offset.MouseInvertVerticalAxis);
        Video.Effects.Specular       = GetBoolean(Offset.VideoEffectsSpecular);
        Video.Effects.Shadows        = GetBoolean(Offset.VideoEffectsShadows);
        Video.Effects.Decals         = GetBoolean(Offset.VideoEffectsDecals);
        Audio.EAX                    = GetBoolean(Offset.AudioEAX);
        Audio.HWA                    = GetBoolean(Offset.AudioHWA);

        /** Keyboard Bindings */
        Input.KeyboardMapping = new Dictionary<ProfileInput.Action, Keyboard>();

        foreach (var button in Enum.GetValues(typeof(Keyboard)))
        {
          reader.BaseStream.Seek((int) button, SeekOrigin.Begin);

          var key = (ProfileInput.Action) reader.ReadUInt16();
          var value = (Keyboard) button;

          /* Skip unassigned input */
          if (key == (ProfileInput.Action) 0x7fff)
          { continue; }

          if (!Input.KeyboardMapping.ContainsKey(key))
            Input.KeyboardMapping.Add(key, value);
        }


        /** Mouse Bindings */
        Input.MouseMapping = new Dictionary<ProfileInput.Action, Mouse>();

        foreach (var input in Enum.GetValues(typeof(Mouse)))
        {
          reader.BaseStream.Seek((int) input, SeekOrigin.Begin);

          var key = (ProfileInput.Action) reader.ReadUInt16();
          var value = (Mouse) input;

          /* Skip unassigned input */
          if (key == (ProfileInput.Action) 0x7fff)
          { continue; }

          if (!Input.MouseMapping.ContainsKey(key))
            Input.MouseMapping.Add(key, value);
        }

        /** Gamepad Bindings */
        Input.GP0_Mapping = new Dictionary<ProfileInput.Action, GP0_Input>();
        Input.GP1_Mapping = new Dictionary<ProfileInput.Action, GP1_Input>();
        Input.GP2_Mapping = new Dictionary<ProfileInput.Action, GP2_Input>();
        Input.GP3_Mapping = new Dictionary<ProfileInput.Action, GP3_Input>();

        foreach (var button in Enum.GetValues(typeof(GP0_Input)))
        {
          reader.BaseStream.Seek((int) button, SeekOrigin.Begin);

          var key   = (ProfileInput.Action) reader.ReadUInt16();
          var value = (GP0_Input) button;

          /* Skip unassigned input */
          if (key == (ProfileInput.Action) 0x7fff)
          { continue; }

          if (!Input.GP0_Mapping.ContainsKey(key))
            Input.GP0_Mapping.Add(key, value);
        }

        foreach (var button in Enum.GetValues(typeof(GP1_Input)))
        {
          reader.BaseStream.Seek((int) button, SeekOrigin.Begin);

          var key   = (ProfileInput.Action) reader.ReadUInt16();
          var value = (GP1_Input) button;

          /* Skip unassigned input */
          if (key == (ProfileInput.Action) 0x7fff)
          { continue; }

          if (!Input.GP1_Mapping.ContainsKey(key))
            Input.GP1_Mapping.Add(key, value);
        }

        foreach (var button in Enum.GetValues(typeof(GP2_Input)))
        {
          reader.BaseStream.Seek((int) button, SeekOrigin.Begin);

          var key   = (ProfileInput.Action) reader.ReadUInt16();
          var value = (GP2_Input) button;

          /* Skip unassigned input */
          if (key == (ProfileInput.Action) 0x7fff)
          { continue; }

          if (!Input.GP2_Mapping.ContainsKey(key))
            Input.GP2_Mapping.Add(key, value);
        }

        foreach (var button in Enum.GetValues(typeof(GP3_Input)))
        {
          reader.BaseStream.Seek((int) button, SeekOrigin.Begin);

          var key   = (ProfileInput.Action) reader.ReadUInt16();
          var value = (GP3_Input) button;

          /* Skip unassigned input */
          if (key == (ProfileInput.Action) 0x7fff)
          { continue; }

          if (!Input.GP3_Mapping.ContainsKey(key))
            Input.GP3_Mapping.Add(key, value);
        }

        /** Gamepad Menu Bindings */

        Input.Gamepads_Menu = new Dictionary<GamePadMenu, DIButtons_Values>();

        foreach (var offset in Enum.GetValues(typeof(GamePadMenu)))
        {
          reader.BaseStream.Seek((int) offset, SeekOrigin.Begin);

          var key   = (GamePadMenu) offset;
          var value = (DIButtons_Values) reader.ReadUInt16();

          /* Skip unassigned input */
          if (value == (DIButtons_Values) 0xffff)
          { continue; }

          if (!Input.Gamepads_Menu.ContainsKey(key))
            Input.Gamepads_Menu.Add(key, value);
        }

        if ((int) Details.Colour == 0xFF)
          Details.Colour = ColourOptions.White;

        Info("Profile deserialisation routine is complete");
      }
    }

    /// <summary>
    ///   Returns object representing the HCE profile detected on the filesystem.
    /// </summary>
    /// <returns>
    ///   Object representing the HCE profile detected on the filesystem.
    /// </returns>
    /// <exception cref="FileNotFoundException">
    ///   lastprof.txt does not exist.
    ///   - or -
    ///   blam.sav does not exist.
    /// </exception>
    public static Profile Detect()
    {
      return Detect(Paths.HCE.Directory);
    }

    /// <summary>
    ///   Returns object representing the HCE profile detected on the filesystem.
    /// </summary>
    /// <returns>
    ///   Object representing the HCE profile detected on the filesystem.
    /// </returns>
    /// <exception cref="FileNotFoundException">
    ///   lastprof.txt does not exist.
    ///   - or -
    ///   blam.sav does not exist.
    /// </exception>
    public static Profile Detect(string directory)
    {
      var lastprof = (LastProfile) Custom.LastProfile(directory);

      if (!lastprof.Exists())
      {
        throw new FileNotFoundException("Cannot detect profile - lastprof.txt does not exist.");
      }
      lastprof.Load();

      var profile = (Profile) Custom.Profile(directory, lastprof.Profile);

      if (!profile.Exists())
      {
        throw new FileNotFoundException("Cannot load detected profile - its blam.sav does not exist.");
      }

      profile.Load();

      return profile;
    }

    /// <summary>
    ///   Returns a list of profiles representing the blam.sav files found in the specified directory.
    /// </summary>
    /// <param name="directory">
    ///   Directory to look for.
    /// </param>
    /// <returns>
    ///   List of Profile instances.
    /// </returns>
    /// <exception cref="DirectoryNotFoundException">
    ///   Provided profiles directory does not exist.
    /// </exception>
    public static List<Profile> List(string directory)
    {
      if (!Directory.Exists(directory))
        throw new DirectoryNotFoundException("Provided profiles directory does not exist.");

      var profiles = new List<Profile>();

      foreach (var current in Directory.GetFiles(directory, "blam.sav", AllDirectories))
      {
        var profile = (Profile) current;
        profile.Load();
        profiles.Add(profile);
      }

      return profiles;
    }

    /// <summary>
    ///   Returns a list of profiles representing the blam.sav files found in the specified directory.
    /// </summary>
    /// <returns>
    ///   List of Profile instances.
    /// </returns>
    public static List<Profile> List()
    {
      return List(Paths.HCE.Profiles);
    }

    /// <summary>
    ///   Represents the inbound object as a string.
    /// </summary>
    /// <param name="profile">
    ///   Object to represent as string.
    /// </param>
    /// <returns>
    ///   String representation of the inbound object.
    /// </returns>
    public static implicit operator string(Profile profile)
    {
      return profile.Path;
    }

    /// <summary>
    ///   Represents the inbound string as an object.
    /// </summary>
    /// <param name="path">
    ///   String to represent as object.
    /// </param>
    /// <returns>
    ///   Object representation of the inbound string.
    /// </returns>
    public static explicit operator Profile(string path)
    {
      return new Profile
      {
        Path = path
      };
    }

    /// <summary>
    ///   Offsets for the data stored in the blam.sav binary.
    /// </summary>
    private enum Offset
    {
      /** values as writen by haloce.exe.
       * Shorts shown here as LE; write as BE.
       * Value types are Uint16/Short where unspecified.
       * Where _unbound, 0x7fff (32,767) is written until the Position reaches the next notable offset.
       */
        _0x0000                  = 0x0000, // 0x0009
      ProfileName                = 0x0002, // "New001"  UTF-16 String
      ProfileColour              = 0x011A, // 0xffff
        _0x011C                  = 0x011C, // 0x1 (default_profile/00.sav)
        _0x011E                  = 0x011E, // 0x0; possible value: 0x08 (from Retail)
        _0x011F                  = 0x011F, // 0x0; possible value: 0x0A (from Retail)
        _0x0120                  = 0x0120, // 0x0; possible value: 0x08 (from Retail)
        _0x0121                  = 0x0121, // 0x0; possible value: 0x08 (from Retail)
        _0x0122                  = 0x0122, // 0x0; possible value: 0x08 (from Retail)
        _0x0123                  = 0x0123, // 0x0; possible value: 0x08 (from Retail)
        _0x0124                  = 0x0124, // 0x0; possible value: 0x08 (from Retail)
        _0x0125                  = 0x0125, // 0x0; possible value: 0x08 (from Retail)
        _0x0126                  = 0x0126, // 0x0; possible value: 0x08 (from Retail)
        _0x0127                  = 0x0127, // 0x0
        _0x0128                  = 0x0128, // 0x0; possible value: 07 (from Retail)
        _0x012E                  = 0x012E, // 0x03
      MouseInvertVerticalAxis    = 0x012F, // 0x00
      /** Input.Keyboard 0x134-0x20D */
      /* empty space filled with 0x7fff*/

      /** Input.Mouse 0x20E-0x220 */
      /* empty space filled with 0x7fff*/

      /** Input.GamePadMenu 0x32A-0x339 */
      /* unbound is 0xffff */

      _0x033A_unbound            = 0x033A, //   0x7fff

      // padding                 = 0x093A, //   0x0

      _0x093E                    = 0x093E, // 0x3f80
      _0x0940                    = 0x0940, // 0x0000
      _0x0942                    = 0x0942, // 0x3f80
      _0x0944                    = 0x0944, // 0x04fc
      _0x0946                    = 0x0946, // 0x3e41
      _0x0948                    = 0x0948, // 0x04fc
      _0x094A                    = 0x094A, // 0x3e41
      _0x094C                    = 0x094C, // 0x0000
      _0x094E                    = 0x094E, // 0x4300
      _0x0950                    = 0x0950, // 0x0000
      _0x0952                    = 0x0952, // 0x00
      _0x0953                    = 0x0953, // 0x43
      MouseSensitivityHorizontal = 0x0954,
      MouseSensitivityVertical   = 0x0955,
      GP0SensitivityHorizontal   = 0x0956,
      GP1SensitivityHorizontal   = 0x0957,
      GP2SensitivityHorizontal   = 0x0958,
      GP3SensitivityHorizontal   = 0x0959,
      GP0SensitivityVertical     = 0x095A,
      GP1SensitivityVertical     = 0x095B,
      GP2SensitivityVertical     = 0x095C,
      GP3SensitivityVertical     = 0x095D,
      _0x095E                    = 0x095E, // 0x0000
      _0x0960                    = 0x0960, // 0x0000
      _0x0962                    = 0x0962, // 0x3f40
      _0x0964                    = 0x0964, // 0x0000
      _0x0966                    = 0x0966, // 0x3f40
      _0x0968                    = 0x0968, // 0x0000
      // padding                 = 0x096a, //   0x0

      VideoResolutionWidth       = 0x0A68, // 0x0780
      VideoResolutionHeight      = 0x0A6A, // 0x0438
      VideoRefreshRate           = 0x0A6C, //
      _0x0A6E                    = 0x0A6E, // 0x02
      VideoFrameRate             = 0x0A6F, // 0x02
      VideoEffectsSpecular       = 0x0A70, // 0x0101
      VideoEffectsShadows        = 0x0A71, // 0x0201
      VideoEffectsDecals         = 0x0A72, // 0x0202
      VideoQualityParticles      = 0x0A73, // 0x0001
      VideoQualityTextures       = 0x0A74,
      VideoMiscellaneousGamma    = 0x0A76,
      // padding                 = 0x0A78, //   0x0

      AudioVolumeMaster          = 0x0B78, // 0x0a
      AudioVolumeEffects         = 0x0B79, // 0x0a
      AudioVolumeMusic           = 0x0B7A, // 0x06
      AudioEAX                   = 0x0B7B, // 0x00
      AudioHWA                   = 0x0B7C, // 0x00
      AudioQuality               = 0x0B7D, // 0x01
      _0x0B7E                    = 0x0B7E, // 0x00
      AudioVariety               = 0x0B7F, // 0x02
      _0x0B80_Padding            = 0x0B80, //   0x0

      _0x0C80                    = 0x0C80, // 0x0103
      _0x0C82                    = 0x0C82, // 0x0001
      _0x0C84                    = 0x0C84, // 0x0000
      _0x0C86                    = 0x0C86, // 0x0101
      // padding                 = 0x0C88, //   0x0

      NetworkServerName          = 0x0D8C, // UTF-16 String. Null-terminated. 31 characters, excluding null.

      NetworkPassword            = 0x0EAC, // UTF-16 String. Null-terminated. 8 characters, excluding null.

      NetworkMaxPlayers          = 0x0EBF, // 0x03 uint8
      // padding                 = 0x0EC0, //   0x0

      NetworkConnectionType      = 0x0FC0, // 0x01 uint8
      NetworkServerAddress       = 0x0FC2, // UTF-16 String. Null-terminated. 31 characters, excluding null.

      NetworkPortServer          = 0x1002, // 0x08fe
      NetworkPortClient          = 0x1004, // 0x08ff
      // padding                 = 0x1006, //   0x0

      Gamepad0_Name              = 0x1108, // UTF-16 String; Can be LE ("Xbox Controller S via XBCD") or BE ("Xbox 360 Controller For Windows")
      // padding
      Gamepad0_Vendor            = 0x1314, // 4-digit hex; e.g. 0x5e04; 045E == Microsoft;
      Gamepad0_Product           = 0x1316, // 4-digit hex; e.g. 0x8902; 0289 == XBCD Xbox Controller;
      Gamepad0_padding           = 0x1318, // 0x0000, 0x0000, 0x0000
      Gamepad0_PIDVID            = 0x131E, // "PIDVID"  UTF-8 String
      Gamepad0_DupeID            = 0x1324, // 0x00; If duplicate, then 0x01 and so on.
      // padding                 = 0x1326, // 0x00 00 00

      Gamepad1_Name              = 0x1328, // UTF-16 String; BE or LE
      // padding
      Gamepad1_Vendor            = 0x1534, // 4-digit hex; e.g. 0x6d04; 046D == Razer?  - Doesn't match DevMgr
      Gamepad1_Product           = 0x1536, // 4-digit hex; e.g. 0x1dc2; C21D == Serval? - Doesn't match DevMgr
      Gamepad1_padding           = 0x1538,
      Gamepad1_PIDVID            = 0x153E,
      Gamepad1_DupeID            = 0x1544,
      // padding

      Gamepad2_Name              = 0x1548,
      // padding
      Gamepad2_Vendor            = 0x1754,
      Gamepad2_Product           = 0x1756,
      Gamepad2_padding           = 0x1758,
      Gamepad2_PIDVID            = 0x175E,
      Gamepad2_DupeID            = 0x1764,
      // padding

      Gampead3_Name              = 0x1768,
      // padding
      Gampead3_Vendor            = 0x1974,
      Gamepad3_Product           = 0x1976,
      Gamepad3_padding           = 0x1978,
      Gamepad3_PIDVID            = 0x197E,

      // a few more left

      BinaryCrc32Hash            = 0x1FFC
    }

    public class ProfileDetails
    {
      public enum ColourOptions
      {
        White  = 0x00, /* universal ui => snow */
        Black  = 0x01, /* universal ui => black */
        Red    = 0x02, /* universal ui => crimson */
        Blue   = 0x03, /* universal ui => blue */
        Gray   = 0x04, /* universal ui => steel */
        Yellow = 0x05, /* universal ui => gold */
        Green  = 0x06, /* universal ui => green */
        Pink   = 0x07, /* universal ui => rose */
        Purple = 0x0A, /* universal ui => violet */
        Cyan   = 0x0B, /* universal ui => cyan */
        Cobalt = 0x0C, /* universal ui => cobalt */
        Orange = 0x0D, /* universal ui => orange */
        Teal   = 0x0E, /* universal ui => aqua */
        Sage   = 0x0F, /* universal ui => sage */
        Brown  = 0x10, /* universal ui => brown */
        Tan    = 0x11, /* universal ui => tan */
        Maroon = 0x14, /* universal ui => maroon */
        Salmon = 0x15, /* universal ui => peach */

        //Random = 0xffff /* Default Value */
      }

      public string        Name   { get; set; } = "New001";            /* default value */
      public ColourOptions Colour { get; set; } = ColourOptions.White; /* default value */
    }

    public class ProfileVideo
    {
      public enum VideoFrameRate
      {
        VsyncOff = 0x00,
        VsyncOn  = 0x01,
        Fps30    = 0x02
      }

      public enum VideoParticles
      {
        Off  = 0x00,
        Low  = 0x01,
        High = 0x02
      }

      public enum VideoQuality
      {
        Low    = 0x00,
        Medium = 0x01,
        High   = 0x02
      }

      public VideoResolution Resolution  { get; set; } = new VideoResolution();
      public VideoEffects    Effects     { get; set; } = new VideoEffects();
      public byte            RefreshRate { get; set; } = 60;                   /* default value */
      public byte            Gamma       { get; set; }                         /* unknown value */
      public VideoFrameRate  FrameRate   { get; set; } = VideoFrameRate.Fps30; /* default value */
      public VideoParticles  Particles   { get; set; } = VideoParticles.High;  /* default value */
      public VideoQuality    Quality     { get; set; } = VideoQuality.High;    /* default value */

      public class VideoResolution
      {
        public ushort Width  { get; set; } = 800; /* default value */
        public ushort Height { get; set; } = 600; /* default value */
      }

      public class VideoEffects
      {
        public bool Specular { get; set; } = true; /* default value */
        public bool Shadows  { get; set; } = true; /* default value */
        public bool Decals   { get; set; } = true; /* default value */
      }
    }

    public class ProfileAudio
    {
      public enum AudioQuality
      {
        Low    = 0x00,
        Normal = 0x01,
        High   = 0x02
      }

      public enum AudioVariety
      {
        Low    = 0x00,
        Medium = 0x01,
        High   = 0x02
      }

      public AudioVolume  Volume  { get; set; } = new AudioVolume();
      public AudioQuality Quality { get; set; } = AudioQuality.Normal; /* default value */
      public AudioVariety Variety { get; set; } = AudioVariety.High;   /* default value */
      public bool         EAX     { get; set; }
      public bool         HWA     { get; set; }

      public class AudioVolume
      {
        public byte Effects { get; set; } = 10; /* default value */
        public byte Master  { get; set; } = 10; /* default value */
        public byte Music   { get; set; } = 6;  /* default value */
      }
    }

    public class ProfileMouse
    {
      public bool             InvertVerticalAxis { get; set; } = false; /* default value */
      public MouseSensitivity Sensitivity        { get; set; } = new MouseSensitivity();

      public class MouseSensitivity
      {
        public byte Horizontal { get; set; } = 3; /* default value */
        public byte Vertical   { get; set; } = 3; /* default value */
      }
    }

    public class ProfileGamepad
    {
      public bool /* Exists? */  InvertVerticalAxis { get; set; } = false;
      public JoystickSensitivity Sensitivity        { get; set; } = new JoystickSensitivity();

      public class JoystickSensitivity
      {
        public byte Horizontal { get; set; } = 3; /* default value */
        public byte Vertical   { get; set; } = 3; /* default value */
      }
    }

    public class ProfileNetwork
    {
      public enum NetworkConnection
      {
        DialUp     = 0x00,
        DslLow     = 0x01,
        DslAverage = 0x02,
        DslHigh    = 0x03,
        Lan        = 0x04
      }

      public NetworkConnection Connection { get; set; } = NetworkConnection.Lan; /* default value */
      public NetworkPort       Port       { get; set; } = new NetworkPort();

      public class NetworkPort
      {
        public ushort Server { get; set; } = 2302; /* default value */
        public ushort Client { get; set; } = 2303; /* default value */
      }
    }

    public class ProfileInput
    {
      /// <summary>
      /// Actions written to input offsets.
      /// Applicable to keyboards, mice, gamepads, and other input devices.
      /// </summary>
      /// <remarks>
      /// Values are written as Little-Endian unsigned Shorts
      /// </remarks>
      public enum Action
      {
        Jump            = 0x00, /* actions  */
        SwitchGrenade   = 0x01, /* combat   */
        Action          = 0x02, /* actions  */
        SwitchWeapon    = 0x03, /* combat   */
        MeleeAttack     = 0x04, /* combat   */
        Flashlight      = 0x05, /* actions  */
        ThrowGrenade    = 0x06, /* combat   */
        FireWeapon      = 0x07, /* combat   */
        MenuAccept      = 0x08, /* misc.    */ /* Keyboard-only */
        MenuBack        = 0x09, /* misc.    */ /* Keyboard-only */
        Crouch          = 0x0A, /* actions  */
        ScopeZoom       = 0x0B, /* actions  */
        ShowScores      = 0x0C, /* misc.    */
        Reload          = 0x0D, /* combat   */
        ExchangeWeapon  = 0x0E, /* combat   */
        Say             = 0x0F, /* misc.    */
        SayToTeam       = 0x10, /* misc.    */
        SayToVehicle    = 0x11, /* misc.    */
        Screenshot      = 0x12,
        MoveForward     = 0x13, /* movement */
        MoveBackward    = 0x14, /* movement */
        MoveLeft        = 0x15, /* movement */
        MoveRight       = 0x16, /* movement */
        LookUp          = 0x17, /* movement */
        LookDown        = 0x18, /* movement */
        LookLeft        = 0x19, /* movement */
        LookRight       = 0x1A, /* movement */
        ShowRules       = 0x1B, /* misc.    */
        ShowPlayerNames = 0x1C  /* misc.    */
      }

      /// <summary>
      /// blam.sav keyboard offsets.<br/>
      /// 0x0134-0x020D
      /// </summary>
      public enum Keyboard
      {
        Escape        = 0x134, // 0x09 MenuBack (unchangeable), Pause (hardcoded)
        F1            = 0x136, // 0x0C ShowScores
        F2            = 0x138, // 0x1B ShowRules
        F3            = 0x13A, // 0x1C ShowPlayerNames
        F4            = 0x13C,
        F5            = 0x13E,
        F6            = 0x140,
        F7            = 0x142,
        F8            = 0x144,
        F9            = 0x146,
        F10           = 0x148,
        F11           = 0x14A,
        F12           = 0x14C,
        Printscreen   = 0x14E, // 0x12 Screenshot
        ScrollLock    = 0x150,
        PauseBreak    = 0x152,
        Grave         = 0x154,
        NumRow1       = 0x156,
        NumRow2       = 0x158,
        NumRow3       = 0x15A,
        NumRow4       = 0x15C,
        NumRow5       = 0x15E,
        NumRow6       = 0x160,
        NumRow7       = 0x162,
        NumRow8       = 0x164,
        NumRow9       = 0x166,
        NumRow0       = 0x168,
        EnDash        = 0x16A,
        Equals        = 0x16C,
        Backspace     = 0x16E,
        Tab           = 0x170, // 0x03 SwitchWeapon
        Q             = 0x172, // 0x05 Flashlight
        W             = 0x174, // 0x13 MoveForward
        E             = 0x176, // 0x02 Action
        R             = 0x178, // 0x0D Reload
        T             = 0x17A, // 0x0F Say
        Y             = 0x17C, // 0x10 SayToTeam
        U             = 0x17E,
        I             = 0x180,
        O             = 0x182,
        P             = 0x184,
        BracketL      = 0x186,
        BracketR      = 0x188,
        Backslash     = 0x18A,
        CapsLock      = 0x18C,
        A             = 0x18E, // 0x15 MoveLeft
        S             = 0x190, // 0x14 MoveBackward
        D             = 0x192, // 0x16 MoveRight
        F             = 0x194, // 0x04 MeleeAttack
        G             = 0x196, // 0x01 SwitchGrenade
        H             = 0x198, // 0x11 SayToVehicle
        J             = 0x19A,
        K             = 0x19C,
        L             = 0x19E,
        SemiColon     = 0x1A0,
        Apostrophe    = 0x1A2,
        Enter         = 0x1A4, // 0x08 MenuAccept (Keyboard only) (unchangeable)
        ShiftL        = 0x1A6,
        Z             = 0x1A8, // 0x0B ScopeZoom
        X             = 0x1AA, // 0x0E ExchangeWeapon
        C             = 0x1AC,
        V             = 0x1AE,
        B             = 0x1B0,
        N             = 0x1B2,
        M             = 0x1B4,
        Comma         = 0x1B6,
        Period        = 0x1B8,
        Slash         = 0x1BA,
        RShift        = 0x1BC,
        LCtrl         = 0x1BE, // 0x0A Crouch
        LWin          = 0x1C0,
        LAlt          = 0x1C2,
        Space         = 0x1C4, // 0x00 Jump
        RAlt          = 0x1C6,
        RWin          = 0x1C8, // (unchangeable)
        Menu          = 0x1CA, // (unchangeable)
        RCtrl         = 0x1CC,
        UpArrow       = 0x1CE,
        DownArrow     = 0x1D0,
        LeftArrow     = 0x1D2,
        RightArrow    = 0x1D4,
        Insert        = 0x1D6,
        Home          = 0x1D8,
        PgUp          = 0x1DA,
        Delete        = 0x1DC,
        End           = 0x1DE,
        PgDown        = 0x1E0,
        NumLock       = 0x1E2, // (unchangeable)
        KPDivide      = 0x1E4,
        KPMultiply    = 0x1E6,
        Keypad0       = 0x1E8,
        Keypad1       = 0x1EA,
        Keypad2       = 0x1EC,
        Keypad3       = 0x1EE,
        Keypad4       = 0x1F0,
        Keypad5       = 0x1F2,
        Keypad6       = 0x1F4,
        Keypad7       = 0x1F6, // Binding this to Jump changed 0x11a from 0xffff to 0x0000
        Keypad8       = 0x1F8,
        Keypad9       = 0x1FA,
        KeypadMinus   = 0x1FC,
        KeypadPlus    = 0x1FE,
        _0x200        = 0x200, // probably KeyPadEnter
        KeypadDecimal = 0x202,
        _0x204        = 0x204,
        _0x206        = 0x206,
        _0x208        = 0x208,
        _0x20A        = 0x20A,
        _0x20C        = 0x20C
        // One of last six unknowns is KeypadEnter
      }

      public enum Mouse
      {
        LeftButton         = 0x020E, // FireWeapon
        MiddleButton       = 0x0210, //
        RightButton        = 0x0212, // ThrowGrenade
        Button4            = 0x0214, // Typically "Browser Back"
        Button5            = 0x0216, // Typically "Browser Forward"
        Button6            = 0x0218,
        Button7            = 0x021A,
        Button8            = 0x021C,
        HAxis_Neg          = 0x021E, // Look Right
        HAxis_Pos          = 0x0220, // Look Left
        VAxis_Neg          = 0x0222, // Look Up
        VAxis_Pos          = 0x0224, // Look Down
        Wheel_Neg          = 0x0226,
        Wheel_Pos          = 0x0228
      }

      public enum GP0_Input
      {
        Button_0  = 0x22A, /* face - button a                 - DI Button 0 */
        Button_1  = 0x22C, /* face - button b                 - DI Button 1 */
        Button_2  = 0x22E, /* face - button x                 - DI Button 2 */
        Button_3  = 0x230, /* face - button y                 - DI Button 3 */
        Button_4  = 0x232, /* shoulder - bumper - left        - DI Button 4 */
        Button_5  = 0x234, /* shoulder - bumper - right       - DI Button 5 */
        Button_6  = 0x236, /* home - back                     - DI Button 6 */
        Button_7  = 0x238, /* home - start                    - DI Button 7 */
        Button_8  = 0x23A, /* analogue - left stick - middle  - DI Button 8 */
        Button_9  = 0x23C, /* analogue - right stick - middle - DI Button 9 */
        Button_10 = 0x23E,

        Axis_1_p = 0x33A, /* analogue - left stick - down    - DI Axis 1 + */
        Axis_1_n = 0x33C, /* analogue - left stick - up      - DI Axis 1 - */
        Axis_2_p = 0x33E, /* analogue - left stick - right   - DI Axis 2 + */
        Axis_2_n = 0x340, /* analogue - left stick - left    - DI Axis 2 - */
        Axis_3_p = 0x342, /* analogue - right stick - down   - DI Axis 3 + */
        Axis_3_n = 0x344, /* analogue - right stick - up     - DI Axis 3 - */
        Axis_4_p = 0x346, /* analogue - right stick - right  - DI Axis 4 + */
        Axis_4_n = 0x348, /* analogue - right stick - left   - DI Axis 4 - */
        Axis_5_p = 0x34A, /* shoulder - trigger - left       - DI Axis 5 + */
        Axis_5_n = 0x34C, /* shoulder - trigger - right      - DI Axis 5 - */
        Axis_6_p = 0x34E,
        Axis_6_n = 0x350,

        DPU   = 0x53A, /* directional - up    */
        DPR   = 0x53E, /* directional - right */
        DPD   = 0x542, /* directional - down  */
        DPL   = 0x546, /* directional - left  */
      }

      public enum GP1_Input
      {
        // Previous Gamepad + 0x40
        Button_0  = GP0_Input.Button_0  + 0x40, /* 0x26A - face - button a                 */
        Button_1  = GP0_Input.Button_1  + 0x40, /* 0x26C - face - button b                 */
        Button_2  = GP0_Input.Button_2  + 0x40, /* 0x26E - face - button x                 */
        Button_3  = GP0_Input.Button_3  + 0x40, /* 0x270 - face - button y                 */
        Button_4  = GP0_Input.Button_4  + 0x40, /* 0x272 - shoulder - bumper - left        */
        Button_5  = GP0_Input.Button_5  + 0x40, /* 0x274 - shoulder - bumper - right       */
        Button_6  = GP0_Input.Button_6  + 0x40, /* 0x276 - home - back                     */
        Button_7  = GP0_Input.Button_7  + 0x40, /* 0x278 - home - start                    */
        Button_8  = GP0_Input.Button_8  + 0x40, /* 0x27A - analogue - left stick - middle  */
        Button_9  = GP0_Input.Button_9  + 0x40, /* 0x27C - analogue - right stick - middle */
        Button_10 = GP0_Input.Button_10 + 0x40, /* 0x27E - Guide */

        // Previous Gamepad + 0x80
        Axis_1_p = GP0_Input.Axis_1_p + 0x80, /* 0x3BA - analogue - left stick - down   */
        Axis_1_n = GP0_Input.Axis_1_n + 0x80, /* 0x3BC - analogue - left stick - up     */
        Axis_2_p = GP0_Input.Axis_2_p + 0x80, /* 0x3BE - analogue - left stick - right  */
        Axis_2_n = GP0_Input.Axis_2_n + 0x80, /* 0x3C0 - analogue - left stick - left   */
        Axis_3_p = GP0_Input.Axis_3_p + 0x80, /* 0x3C2 - analogue - right stick - down  */
        Axis_3_n = GP0_Input.Axis_3_n + 0x80, /* 0x3C4 - analogue - right stick - up    */
        Axis_4_p = GP0_Input.Axis_4_p + 0x80, /* 0x3C6 - analogue - right stick - right */
        Axis_4_n = GP0_Input.Axis_4_n + 0x80, /* 0x3C8 - analogue - right stick - left  */
        Axis_5_p = GP0_Input.Axis_5_p + 0x80, /* 0x3CA - shoulder - trigger - left      */
        Axis_5_n = GP0_Input.Axis_5_n + 0x80, /* 0x3CC - shoulder - trigger - right     */
        Axis_6_p = GP0_Input.Axis_6_p + 0x80, /* 0x3CE */
        Axis_6_n = GP0_Input.Axis_6_n + 0x80, /* 0x3D0 */

        // Previous Gamepad + 0x100
        DPU = GP0_Input.DPU + 0x100, /* 0x63A - directional - up    */
        DPR = GP0_Input.DPR + 0x100, /* 0x63E - directional - right */
        DPD = GP0_Input.DPD + 0x100, /* 0x642 - directional - down  */
        DPL = GP0_Input.DPL + 0x100, /* 0x646 - directional - left  */
      }

      public enum GP2_Input
      {
        // Previous Gamepad + 0x40
        Button_0  = GP1_Input.Button_0  + 0x40, /* 0x2AA - face - button a                 */
        Button_1  = GP1_Input.Button_1  + 0x40, /* 0x2AC - face - button b                 */
        Button_2  = GP1_Input.Button_2  + 0x40, /* 0x2AE - face - button x                 */
        Button_3  = GP1_Input.Button_3  + 0x40, /* 0x2B0 - face - button y                 */
        Button_4  = GP1_Input.Button_4  + 0x40, /* 0x2B2 - shoulder - bumper - left        */
        Button_5  = GP1_Input.Button_5  + 0x40, /* 0x2B4 - shoulder - bumper - right       */
        Button_6  = GP1_Input.Button_6  + 0x40, /* 0x2B6 - home - back                     */
        Button_7  = GP1_Input.Button_7  + 0x40, /* 0x2B8 - home - start                    */
        Button_8  = GP1_Input.Button_8  + 0x40, /* 0x2BA - analogue - left stick - middle  */
        Button_9  = GP1_Input.Button_9  + 0x40, /* 0x2BC - analogue - right stick - middle */
        Button_10 = GP1_Input.Button_10 + 0x40, /* 0x2BE - Guide */

        // Previous Gamepad + 0x80
        Axis_1_p = GP1_Input.Axis_1_p + 0x80, /* 0x43A - analogue - left stick - down   */
        Axis_1_n = GP1_Input.Axis_1_n + 0x80, /* 0x43C - analogue - left stick - up     */
        Axis_2_p = GP1_Input.Axis_2_p + 0x80, /* 0x43E - analogue - left stick - right  */
        Axis_2_n = GP1_Input.Axis_2_n + 0x80, /* 0x440 - analogue - left stick - left   */
        Axis_3_p = GP1_Input.Axis_3_p + 0x80, /* 0x442 - analogue - right stick - down  */
        Axis_3_n = GP1_Input.Axis_3_n + 0x80, /* 0x444 - analogue - right stick - up    */
        Axis_4_p = GP1_Input.Axis_4_p + 0x80, /* 0x446 - analogue - right stick - right */
        Axis_4_n = GP1_Input.Axis_4_n + 0x80, /* 0x448 - analogue - right stick - left  */
        Axis_5_p = GP1_Input.Axis_5_p + 0x80, /* 0x44A - shoulder - trigger - left      */
        Axis_5_n = GP1_Input.Axis_5_n + 0x80, /* 0x44C - shoulder - trigger - right     */
        Axis_6_p = GP1_Input.Axis_6_p + 0x80, /* 0x44E */
        Axis_6_n = GP1_Input.Axis_6_n + 0x80, /* 0x450 */

        // Previous Gamepad + 0x100
        DPU = GP1_Input.DPU + 0x100, /* 0x73A - directional - up    */
        DPR = GP1_Input.DPR + 0x100, /* 0x73E - directional - right */
        DPD = GP1_Input.DPD + 0x100, /* 0x742 - directional - down  */
        DPL = GP1_Input.DPL + 0x100, /* 0x746 - directional - left  */
      }

      public enum GP3_Input
      {
        // Previous Gamepad + 0x40
        Button_0  = GP2_Input.Button_0  + 0x40, /* 0x2EA - face - button a                 */
        Button_1  = GP2_Input.Button_1  + 0x40, /* 0x2EC - face - button b                 */
        Button_2  = GP2_Input.Button_2  + 0x40, /* 0x2EE - face - button x                 */
        Button_3  = GP2_Input.Button_3  + 0x40, /* 0x2F0 - face - button y                 */
        Button_4  = GP2_Input.Button_4  + 0x40, /* 0x2F2 - shoulder - bumper - left        */
        Button_5  = GP2_Input.Button_5  + 0x40, /* 0x2F4 - shoulder - bumper - right       */
        Button_6  = GP2_Input.Button_6  + 0x40, /* 0x2F6 - home - back                     */
        Button_7  = GP2_Input.Button_7  + 0x40, /* 0x2F8 - home - start                    */
        Button_8  = GP2_Input.Button_8  + 0x40, /* 0x2FA - analogue - left stick - middle  */
        Button_9  = GP2_Input.Button_9  + 0x40, /* 0x2FC - analogue - right stick - middle */
        Button_10 = GP2_Input.Button_10 + 0x40, /* 0x2FE - Guide */

        // Previous Gamepad + 0x80
        Axis_1_p = GP2_Input.Axis_1_p + 0x80, /* 0x4BA - analogue - left stick - down   */
        Axis_1_n = GP2_Input.Axis_1_n + 0x80, /* 0x4BC - analogue - left stick - up     */
        Axis_2_p = GP2_Input.Axis_2_p + 0x80, /* 0x4BE - analogue - left stick - right  */
        Axis_2_n = GP2_Input.Axis_2_n + 0x80, /* 0x4C0 - analogue - left stick - left   */
        Axis_3_p = GP2_Input.Axis_3_p + 0x80, /* 0x4C2 - analogue - right stick - down  */
        Axis_3_n = GP2_Input.Axis_3_n + 0x80, /* 0x4C4 - analogue - right stick - up    */
        Axis_4_p = GP2_Input.Axis_4_p + 0x80, /* 0x4C6 - analogue - right stick - right */
        Axis_4_n = GP2_Input.Axis_4_n + 0x80, /* 0x4C8 - analogue - right stick - left  */
        Axis_5_p = GP2_Input.Axis_5_p + 0x80, /* 0x4CA - shoulder - trigger - left      */
        Axis_5_n = GP2_Input.Axis_5_n + 0x80, /* 0x4CC - shoulder - trigger - right     */
        Axis_6_p = GP2_Input.Axis_6_p + 0x80, /* 0x4CE */
        Axis_6_n = GP2_Input.Axis_6_n + 0x80, /* 0x4D0 */

        // Previous Gamepad + 0x100
        DPU = GP2_Input.DPU + 0x100, /* 0x83A - directional - up    */
        DPR = GP2_Input.DPR + 0x100, /* 0x83E - directional - right */
        DPD = GP2_Input.DPD + 0x100, /* 0x842 - directional - down  */
        DPL = GP2_Input.DPL + 0x100, /* 0x846 - directional - left  */
      }

      /// <summary>
      /// Represents per-controller MenuAccept and MenuBack in Misc<br/>
      /// Offsets from 810=0x32A to 825=0x339.
      /// </summary>
      public enum GamePadMenu
      {
        GP0_MenuAccept = 0x32A,
        GP0_MenuBack   = 0x32C,
        GP1_MenuAccept = 0x32E,
        GP1_MenuBack   = 0x330,
        GP2_MenuAccept = 0x332,
        GP2_MenuBack   = 0x334,
        GP3_MenuAccept = 0x336,
        GP3_MenuBack   = 0x338
      }

      /// <summary>
      /// Buttons for gamepads and similar devices
      /// </summary>
      public enum DIButtons_Values
      {
        Button0  = 0x00, /* face - button a                */
        Button1  = 0x01, /* face - button b                */
        Button2  = 0x02, /* face - button x                */
        Button3  = 0x03, /* face - button y                */
        Button4  = 0x04, /* shoulder - L shoulder, white   */
        Button5  = 0x05, /* shoulder - R shoulder, black   */
        Button6  = 0x06, /* home - back                    */
        Button7  = 0x07, /* home - start                   */
        Button8  = 0x08, /* analogue - left  stick - click */
        Button9  = 0x09, /* analogue - right stick - click */
     // Button11 = 0x0A  /* Test with an X360/1 gamepad's Guide button...or a similar input device.
     // Button12 = 0x0B,
     // Button13 = 0x0C,
     // Button14 = 0x0D,
     // Button15 = 0x0E,
      }

      public Dictionary<Action, Keyboard> KeyboardMapping = new Dictionary<Action, Keyboard>
      {
        /* Defaults as determined by Halo */
        { Action.Jump           , Keyboard.Space },
        { Action.SwitchGrenade  , Keyboard.G },
        { Action.Action         , Keyboard.E },
        { Action.SwitchWeapon   , Keyboard.Tab },
        { Action.MeleeAttack    , Keyboard.F },
        { Action.Flashlight     , Keyboard.Q },
        { Action.MenuAccept     , Keyboard.Enter },
        { Action.MenuBack       , Keyboard.Escape },
        { Action.Crouch         , Keyboard.LCtrl },
        { Action.ScopeZoom      , Keyboard.Z },
        { Action.ShowScores     , Keyboard.F1 },
        { Action.Reload         , Keyboard.R },
        { Action.ExchangeWeapon , Keyboard.X },
        { Action.Say            , Keyboard.T },
        { Action.SayToTeam      , Keyboard.Y },
        { Action.SayToVehicle   , Keyboard.H },
        { Action.Screenshot     , Keyboard.Printscreen },
        { Action.MoveForward    , Keyboard.W },
        { Action.MoveBackward   , Keyboard.S },
        { Action.MoveLeft       , Keyboard.A },
        { Action.MoveRight      , Keyboard.D },
        { Action.ShowRules      , Keyboard.F2 },
        { Action.ShowPlayerNames, Keyboard.F3 },
      };

      public Dictionary<Action, Mouse> MouseMapping = new Dictionary<Action, Mouse>
      {
        { Action.LookUp      , Mouse.VAxis_Neg    },
        { Action.LookDown    , Mouse.VAxis_Pos    },
        { Action.LookLeft    , Mouse.HAxis_Pos    },
        { Action.LookRight   , Mouse.HAxis_Neg    },
        { Action.FireWeapon  , Mouse.LeftButton   },
        { Action.ThrowGrenade, Mouse.RightButton  },
        { Action.ScopeZoom   , Mouse.MiddleButton },
      };

      public Dictionary<Action, GP0_Input> GP0_Mapping = new Dictionary<Action, GP0_Input>();
      public Dictionary<Action, GP1_Input> GP1_Mapping = new Dictionary<Action, GP1_Input>();
      public Dictionary<Action, GP2_Input> GP2_Mapping = new Dictionary<Action, GP2_Input>();
      public Dictionary<Action, GP3_Input> GP3_Mapping = new Dictionary<Action, GP3_Input>();

      public Dictionary<GamePadMenu, DIButtons_Values> Gamepads_Menu = new Dictionary<GamePadMenu, DIButtons_Values>
      {
        { GamePadMenu.GP0_MenuAccept, DIButtons_Values.Button0 },
        { GamePadMenu.GP0_MenuBack  , DIButtons_Values.Button1 },
        { GamePadMenu.GP1_MenuAccept, DIButtons_Values.Button0 },
        { GamePadMenu.GP1_MenuBack  , DIButtons_Values.Button1 },
        { GamePadMenu.GP2_MenuAccept, DIButtons_Values.Button0 },
        { GamePadMenu.GP2_MenuBack  , DIButtons_Values.Button1 },
        { GamePadMenu.GP3_MenuAccept, DIButtons_Values.Button0 },
        { GamePadMenu.GP3_MenuBack  , DIButtons_Values.Button1 },
      };
    }
  }
}