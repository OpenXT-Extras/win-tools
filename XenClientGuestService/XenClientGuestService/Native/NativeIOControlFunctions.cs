//
// Copyright (c) 2013 Citrix Systems, Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace XenClientGuestService.Native
{
    internal static class IOControlFunctions
    {
        [DllImport("Kernel32.dll", SetLastError = false, CharSet = CharSet.Auto)]
        internal static extern bool DeviceIoControl(
            Microsoft.Win32.SafeHandles.SafeFileHandle hDevice,
            EIOControlCode IoControlCode,
            [MarshalAs(UnmanagedType.AsAny)]
            [In] object InBuffer,
            uint nInBufferSize,
            [MarshalAs(UnmanagedType.AsAny)]
            [Out] object OutBuffer,
            uint nOutBufferSize,
            ref uint pBytesReturned,
            [In] IntPtr Overlapped
        );
    }
    [Flags]
    public enum EMethod : uint
    {
        Buffered = 0,
        InDirect = 1,
        OutDirect = 2,
        Neither = 3
    }

    [Flags]
    public enum EFileDevice : uint
    {
        Beep = 0x00000001,
        CDRom = 0x00000002,
        CDRomFileSytem = 0x00000003,
        Controller = 0x00000004,
        Datalink = 0x00000005,
        Dfs = 0x00000006,
        Disk = 0x00000007,
        DiskFileSystem = 0x00000008,
        FileSystem = 0x00000009,
        InPortPort = 0x0000000a,
        Keyboard = 0x0000000b,
        Mailslot = 0x0000000c,
        MidiIn = 0x0000000d,
        MidiOut = 0x0000000e,
        Mouse = 0x0000000f,
        MultiUncProvider = 0x00000010,
        NamedPipe = 0x00000011,
        Network = 0x00000012,
        NetworkBrowser = 0x00000013,
        NetworkFileSystem = 0x00000014,
        Null = 0x00000015,
        ParallelPort = 0x00000016,
        PhysicalNetcard = 0x00000017,
        Printer = 0x00000018,
        Scanner = 0x00000019,
        SerialMousePort = 0x0000001a,
        SerialPort = 0x0000001b,
        Screen = 0x0000001c,
        Sound = 0x0000001d,
        Streams = 0x0000001e,
        Tape = 0x0000001f,
        TapeFileSystem = 0x00000020,
        Transport = 0x00000021,
        Unknown = 0x00000022,
        Video = 0x00000023,
        VirtualDisk = 0x00000024,
        WaveIn = 0x00000025,
        WaveOut = 0x00000026,
        Port8042 = 0x00000027,
        NetworkRedirector = 0x00000028,
        Battery = 0x00000029,
        BusExtender = 0x0000002a,
        Modem = 0x0000002b,
        Vdm = 0x0000002c,
        MassStorage = 0x0000002d,
        Smb = 0x0000002e,
        Ks = 0x0000002f,
        Changer = 0x00000030,
        Smartcard = 0x00000031,
        Acpi = 0x00000032,
        Dvd = 0x00000033,
        FullscreenVideo = 0x00000034,
        DfsFileSystem = 0x00000035,
        DfsVolume = 0x00000036,
        Serenum = 0x00000037,
        Termsrv = 0x00000038,
        Ksec = 0x00000039,
        // From Windows Driver Kit 7
        Fips = 0x0000003A,
        Infiniband = 0x0000003B,
        Vmbus = 0x0000003E,
        CryptProvider = 0x0000003F,
        Wpd = 0x00000040,
        Bluetooth = 0x00000041,
        MtComposite = 0x00000042,
        MtTransport = 0x00000043,
        Biometric = 0x00000044,
        Pmi = 0x00000045
    }

    [Flags]
    public enum EXenInput : uint
    {
        XENINP_FUNC_ACCELERATION = 0x10
    }
    /// <summary>
    /// IO Control Codes
    /// Useful links:
    ///     http://www.ioctls.net/
    ///     http://msdn.microsoft.com/en-us/library/windows/hardware/ff543023(v=vs.85).aspx
    /// </summary>
    [Flags]
    public enum EIOControlCode : uint
    {
        // XenInput
        // #define XENINP_IOCTL_ACCELERATION CTL_CODE(FILE_DEVICE_UNKNOWN, XENINP_FUNC_ACCELERATION, METHOD_BUFFERED, FILE_ANY_ACCESS)
        XENINP_IOCTL_ACCELERATION = (Native.EFileDevice.Unknown << 16) | (EXenInput.XENINP_FUNC_ACCELERATION << 2) | Native.EMethod.Buffered | (0 << 14), // System.IO.FileAccess.Any
        // STORAGE
        StorageCheckVerify = (EFileDevice.MassStorage << 16) | (0x0200 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        StorageCheckVerify2 = (EFileDevice.MassStorage << 16) | (0x0200 << 2) | EMethod.Buffered | (0 << 14), // System.IO.FileAccess.Any
        StorageMediaRemoval = (EFileDevice.MassStorage << 16) | (0x0201 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        StorageEjectMedia = (EFileDevice.MassStorage << 16) | (0x0202 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        StorageLoadMedia = (EFileDevice.MassStorage << 16) | (0x0203 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        StorageLoadMedia2 = (EFileDevice.MassStorage << 16) | (0x0203 << 2) | EMethod.Buffered | (0 << 14),
        StorageReserve = (EFileDevice.MassStorage << 16) | (0x0204 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        StorageRelease = (EFileDevice.MassStorage << 16) | (0x0205 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        StorageFindNewDevices = (EFileDevice.MassStorage << 16) | (0x0206 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        StorageEjectionControl = (EFileDevice.MassStorage << 16) | (0x0250 << 2) | EMethod.Buffered | (0 << 14),
        StorageMcnControl = (EFileDevice.MassStorage << 16) | (0x0251 << 2) | EMethod.Buffered | (0 << 14),
        StorageGetMediaTypes = (EFileDevice.MassStorage << 16) | (0x0300 << 2) | EMethod.Buffered | (0 << 14),
        StorageGetMediaTypesEx = (EFileDevice.MassStorage << 16) | (0x0301 << 2) | EMethod.Buffered | (0 << 14),
        StorageResetBus = (EFileDevice.MassStorage << 16) | (0x0400 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        StorageResetDevice = (EFileDevice.MassStorage << 16) | (0x0401 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        StorageGetDeviceNumber = (EFileDevice.MassStorage << 16) | (0x0420 << 2) | EMethod.Buffered | (0 << 14),
        StoragePredictFailure = (EFileDevice.MassStorage << 16) | (0x0440 << 2) | EMethod.Buffered | (0 << 14),
        StorageObsoleteResetBus = (EFileDevice.MassStorage << 16) | (0x0400 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        StorageObsoleteResetDevice = (EFileDevice.MassStorage << 16) | (0x0401 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        // DISK
        DiskGetDriveGeometry = (EFileDevice.Disk << 16) | (0x0000 << 2) | EMethod.Buffered | (0 << 14),
        DiskGetDriveGeometryEx = (EFileDevice.Disk << 16) | (0x0028 << 2) | EMethod.Buffered | (0 << 14),
        DiskGetPartitionInfo = (EFileDevice.Disk << 16) | (0x0001 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        DiskGetPartitionInfoEx = (EFileDevice.Disk << 16) | (0x0012 << 2) | EMethod.Buffered | (0 << 14),
        DiskSetPartitionInfo = (EFileDevice.Disk << 16) | (0x0002 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        DiskGetDriveLayout = (EFileDevice.Disk << 16) | (0x0003 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        DiskSetDriveLayout = (EFileDevice.Disk << 16) | (0x0004 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        DiskVerify = (EFileDevice.Disk << 16) | (0x0005 << 2) | EMethod.Buffered | (0 << 14),
        DiskFormatTracks = (EFileDevice.Disk << 16) | (0x0006 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        DiskReassignBlocks = (EFileDevice.Disk << 16) | (0x0007 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        DiskPerformance = (EFileDevice.Disk << 16) | (0x0008 << 2) | EMethod.Buffered | (0 << 14),
        DiskIsWritable = (EFileDevice.Disk << 16) | (0x0009 << 2) | EMethod.Buffered | (0 << 14),
        DiskLogging = (EFileDevice.Disk << 16) | (0x000a << 2) | EMethod.Buffered | (0 << 14),
        DiskFormatTracksEx = (EFileDevice.Disk << 16) | (0x000b << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        DiskHistogramStructure = (EFileDevice.Disk << 16) | (0x000c << 2) | EMethod.Buffered | (0 << 14),
        DiskHistogramData = (EFileDevice.Disk << 16) | (0x000d << 2) | EMethod.Buffered | (0 << 14),
        DiskHistogramReset = (EFileDevice.Disk << 16) | (0x000e << 2) | EMethod.Buffered | (0 << 14),
        DiskRequestStructure = (EFileDevice.Disk << 16) | (0x000f << 2) | EMethod.Buffered | (0 << 14),
        DiskRequestData = (EFileDevice.Disk << 16) | (0x0010 << 2) | EMethod.Buffered | (0 << 14),
        DiskControllerNumber = (EFileDevice.Disk << 16) | (0x0011 << 2) | EMethod.Buffered | (0 << 14),
        DiskSmartGetVersion = (EFileDevice.Disk << 16) | (0x0020 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        DiskSmartSendDriveCommand = (EFileDevice.Disk << 16) | (0x0021 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        DiskSmartRcvDriveData = (EFileDevice.Disk << 16) | (0x0022 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        DiskUpdateDriveSize = (EFileDevice.Disk << 16) | (0x0032 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        DiskGrowPartition = (EFileDevice.Disk << 16) | (0x0034 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        DiskGetCacheInformation = (EFileDevice.Disk << 16) | (0x0035 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        DiskSetCacheInformation = (EFileDevice.Disk << 16) | (0x0036 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        DiskDeleteDriveLayout = (EFileDevice.Disk << 16) | (0x0040 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        DiskFormatDrive = (EFileDevice.Disk << 16) | (0x00f3 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        DiskSenseDevice = (EFileDevice.Disk << 16) | (0x00f8 << 2) | EMethod.Buffered | (0 << 14),
        DiskCheckVerify = (EFileDevice.Disk << 16) | (0x0200 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        DiskMediaRemoval = (EFileDevice.Disk << 16) | (0x0201 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        DiskEjectMedia = (EFileDevice.Disk << 16) | (0x0202 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        DiskLoadMedia = (EFileDevice.Disk << 16) | (0x0203 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        DiskReserve = (EFileDevice.Disk << 16) | (0x0204 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        DiskRelease = (EFileDevice.Disk << 16) | (0x0205 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        DiskFindNewDevices = (EFileDevice.Disk << 16) | (0x0206 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        DiskGetMediaTypes = (EFileDevice.Disk << 16) | (0x0300 << 2) | EMethod.Buffered | (0 << 14),
        DiskSetPartitionInfoEx = (EFileDevice.Disk << 16) | (0x0013 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        DiskGetDriveLayoutEx = (EFileDevice.Disk << 16) | (0x0014 << 2) | EMethod.Buffered | (0 << 14),
        DiskSetDriveLayoutEx = (EFileDevice.Disk << 16) | (0x0015 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        DiskCreateDisk = (EFileDevice.Disk << 16) | (0x0016 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        DiskGetLengthInfo = (EFileDevice.Disk << 16) | (0x0017 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        // CHANGER
        ChangerGetParameters = (EFileDevice.Changer << 16) | (0x0000 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        ChangerGetStatus = (EFileDevice.Changer << 16) | (0x0001 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        ChangerGetProductData = (EFileDevice.Changer << 16) | (0x0002 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        ChangerSetAccess = (EFileDevice.Changer << 16) | (0x0004 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        ChangerGetElementStatus = (EFileDevice.Changer << 16) | (0x0005 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        ChangerInitializeElementStatus = (EFileDevice.Changer << 16) | (0x0006 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        ChangerSetPosition = (EFileDevice.Changer << 16) | (0x0007 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        ChangerExchangeMedium = (EFileDevice.Changer << 16) | (0x0008 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        ChangerMoveMedium = (EFileDevice.Changer << 16) | (0x0009 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        ChangerReinitializeTarget = (EFileDevice.Changer << 16) | (0x000A << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        ChangerQueryVolumeTags = (EFileDevice.Changer << 16) | (0x000B << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        // FILESYSTEM
        FsctlRequestOplockLevel1 = (EFileDevice.FileSystem << 16) | (0 << 2) | EMethod.Buffered | (0 << 14),
        FsctlRequestOplockLevel2 = (EFileDevice.FileSystem << 16) | (1 << 2) | EMethod.Buffered | (0 << 14),
        FsctlRequestBatchOplock = (EFileDevice.FileSystem << 16) | (2 << 2) | EMethod.Buffered | (0 << 14),
        FsctlOplockBreakAcknowledge = (EFileDevice.FileSystem << 16) | (3 << 2) | EMethod.Buffered | (0 << 14),
        FsctlOpBatchAckClosePending = (EFileDevice.FileSystem << 16) | (4 << 2) | EMethod.Buffered | (0 << 14),
        FsctlOplockBreakNotify = (EFileDevice.FileSystem << 16) | (5 << 2) | EMethod.Buffered | (0 << 14),
        FsctlLockVolume = (EFileDevice.FileSystem << 16) | (6 << 2) | EMethod.Buffered | (0 << 14),
        FsctlUnlockVolume = (EFileDevice.FileSystem << 16) | (7 << 2) | EMethod.Buffered | (0 << 14),
        FsctlDismountVolume = (EFileDevice.FileSystem << 16) | (8 << 2) | EMethod.Buffered | (0 << 14),
        FsctlIsVolumeMounted = (EFileDevice.FileSystem << 16) | (10 << 2) | EMethod.Buffered | (0 << 14),
        FsctlIsPathnameValid = (EFileDevice.FileSystem << 16) | (11 << 2) | EMethod.Buffered | (0 << 14),
        FsctlMarkVolumeDirty = (EFileDevice.FileSystem << 16) | (12 << 2) | EMethod.Buffered | (0 << 14),
        FsctlQueryRetrievalPointers = (EFileDevice.FileSystem << 16) | (14 << 2) | EMethod.Neither | (0 << 14),
        FsctlGetCompression = (EFileDevice.FileSystem << 16) | (15 << 2) | EMethod.Buffered | (0 << 14),
        FsctlSetCompression = (EFileDevice.FileSystem << 16) | (16 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        FsctlMarkAsSystemHive = (EFileDevice.FileSystem << 16) | (19 << 2) | EMethod.Neither | (0 << 14),
        FsctlOplockBreakAckNo2 = (EFileDevice.FileSystem << 16) | (20 << 2) | EMethod.Buffered | (0 << 14),
        FsctlInvalidateVolumes = (EFileDevice.FileSystem << 16) | (21 << 2) | EMethod.Buffered | (0 << 14),
        FsctlQueryFatBpb = (EFileDevice.FileSystem << 16) | (22 << 2) | EMethod.Buffered | (0 << 14),
        FsctlRequestFilterOplock = (EFileDevice.FileSystem << 16) | (23 << 2) | EMethod.Buffered | (0 << 14),
        FsctlFileSystemGetStatistics = (EFileDevice.FileSystem << 16) | (24 << 2) | EMethod.Buffered | (0 << 14),
        FsctlGetNtfsVolumeData = (EFileDevice.FileSystem << 16) | (25 << 2) | EMethod.Buffered | (0 << 14),
        FsctlGetNtfsFileRecord = (EFileDevice.FileSystem << 16) | (26 << 2) | EMethod.Buffered | (0 << 14),
        FsctlGetVolumeBitmap = (EFileDevice.FileSystem << 16) | (27 << 2) | EMethod.Neither | (0 << 14),
        FsctlGetRetrievalPointers = (EFileDevice.FileSystem << 16) | (28 << 2) | EMethod.Neither | (0 << 14),
        FsctlMoveFile = (EFileDevice.FileSystem << 16) | (29 << 2) | EMethod.Buffered | (0 << 14),
        FsctlIsVolumeDirty = (EFileDevice.FileSystem << 16) | (30 << 2) | EMethod.Buffered | (0 << 14),
        FsctlGetHfsInformation = (EFileDevice.FileSystem << 16) | (31 << 2) | EMethod.Buffered | (0 << 14),
        FsctlAllowExtendedDasdIo = (EFileDevice.FileSystem << 16) | (32 << 2) | EMethod.Neither | (0 << 14),
        FsctlReadPropertyData = (EFileDevice.FileSystem << 16) | (33 << 2) | EMethod.Neither | (0 << 14),
        FsctlWritePropertyData = (EFileDevice.FileSystem << 16) | (34 << 2) | EMethod.Neither | (0 << 14),
        FsctlFindFilesBySid = (EFileDevice.FileSystem << 16) | (35 << 2) | EMethod.Neither | (0 << 14),
        FsctlDumpPropertyData = (EFileDevice.FileSystem << 16) | (37 << 2) | EMethod.Neither | (0 << 14),
        FsctlSetObjectId = (EFileDevice.FileSystem << 16) | (38 << 2) | EMethod.Buffered | (0 << 14),
        FsctlGetObjectId = (EFileDevice.FileSystem << 16) | (39 << 2) | EMethod.Buffered | (0 << 14),
        FsctlDeleteObjectId = (EFileDevice.FileSystem << 16) | (40 << 2) | EMethod.Buffered | (0 << 14),
        FsctlSetReparsePoint = (EFileDevice.FileSystem << 16) | (41 << 2) | EMethod.Buffered | (0 << 14),
        FsctlGetReparsePoint = (EFileDevice.FileSystem << 16) | (42 << 2) | EMethod.Buffered | (0 << 14),
        FsctlDeleteReparsePoint = (EFileDevice.FileSystem << 16) | (43 << 2) | EMethod.Buffered | (0 << 14),
        FsctlEnumUsnData = (EFileDevice.FileSystem << 16) | (44 << 2) | EMethod.Neither | (0 << 14),
        FsctlSecurityIdCheck = (EFileDevice.FileSystem << 16) | (45 << 2) | EMethod.Neither | (System.IO.FileAccess.Read << 14),
        FsctlReadUsnJournal = (EFileDevice.FileSystem << 16) | (46 << 2) | EMethod.Neither | (0 << 14),
        FsctlSetObjectIdExtended = (EFileDevice.FileSystem << 16) | (47 << 2) | EMethod.Buffered | (0 << 14),
        FsctlCreateOrGetObjectId = (EFileDevice.FileSystem << 16) | (48 << 2) | EMethod.Buffered | (0 << 14),
        FsctlSetSparse = (EFileDevice.FileSystem << 16) | (49 << 2) | EMethod.Buffered | (0 << 14),
        FsctlSetZeroData = (EFileDevice.FileSystem << 16) | (50 << 2) | EMethod.Buffered | (System.IO.FileAccess.Write << 14),
        FsctlQueryAllocatedRanges = (EFileDevice.FileSystem << 16) | (51 << 2) | EMethod.Neither | (System.IO.FileAccess.Read << 14),
        FsctlEnableUpgrade = (EFileDevice.FileSystem << 16) | (52 << 2) | EMethod.Buffered | (System.IO.FileAccess.Write << 14),
        FsctlSetEncryption = (EFileDevice.FileSystem << 16) | (53 << 2) | EMethod.Neither | (0 << 14),
        FsctlEncryptionFsctlIo = (EFileDevice.FileSystem << 16) | (54 << 2) | EMethod.Neither | (0 << 14),
        FsctlWriteRawEncrypted = (EFileDevice.FileSystem << 16) | (55 << 2) | EMethod.Neither | (0 << 14),
        FsctlReadRawEncrypted = (EFileDevice.FileSystem << 16) | (56 << 2) | EMethod.Neither | (0 << 14),
        FsctlCreateUsnJournal = (EFileDevice.FileSystem << 16) | (57 << 2) | EMethod.Neither | (0 << 14),
        FsctlReadFileUsnData = (EFileDevice.FileSystem << 16) | (58 << 2) | EMethod.Neither | (0 << 14),
        FsctlWriteUsnCloseRecord = (EFileDevice.FileSystem << 16) | (59 << 2) | EMethod.Neither | (0 << 14),
        FsctlExtendVolume = (EFileDevice.FileSystem << 16) | (60 << 2) | EMethod.Buffered | (0 << 14),
        FsctlQueryUsnJournal = (EFileDevice.FileSystem << 16) | (61 << 2) | EMethod.Buffered | (0 << 14),
        FsctlDeleteUsnJournal = (EFileDevice.FileSystem << 16) | (62 << 2) | EMethod.Buffered | (0 << 14),
        FsctlMarkHandle = (EFileDevice.FileSystem << 16) | (63 << 2) | EMethod.Buffered | (0 << 14),
        FsctlSisCopyFile = (EFileDevice.FileSystem << 16) | (64 << 2) | EMethod.Buffered | (0 << 14),
        FsctlSisLinkFiles = (EFileDevice.FileSystem << 16) | (65 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        FsctlHsmMsg = (EFileDevice.FileSystem << 16) | (66 << 2) | EMethod.Buffered | (System.IO.FileAccess.ReadWrite << 14),
        FsctlNssControl = (EFileDevice.FileSystem << 16) | (67 << 2) | EMethod.Buffered | (System.IO.FileAccess.Write << 14),
        FsctlHsmData = (EFileDevice.FileSystem << 16) | (68 << 2) | EMethod.Neither | (System.IO.FileAccess.ReadWrite << 14),
        FsctlRecallFile = (EFileDevice.FileSystem << 16) | (69 << 2) | EMethod.Neither | (0 << 14),
        FsctlNssRcontrol = (EFileDevice.FileSystem << 16) | (70 << 2) | EMethod.Buffered | (System.IO.FileAccess.Read << 14),
        // VIDEO
        VideoQuerySupportedBrightness = (EFileDevice.Video << 16) | (0x0125 << 2) | EMethod.Buffered | (0 << 14),
        VideoQueryDisplayBrightness = (EFileDevice.Video << 16) | (0x0126 << 2) | EMethod.Buffered | (0 << 14),
        VideoSetDisplayBrightness = (EFileDevice.Video << 16) | (0x0127 << 2) | EMethod.Buffered | (0 << 14)
    }

}
