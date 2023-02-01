namespace UsbDeviceInformationCollectorCore.CLibs.Enums
{
    public enum CrErrorCodes
    {
        Success = 0,
        Default,
        OutOfMemory,
        InvalidPointer,
        InvalidFlag,
        InvalidDevNode,
        InvalidResDes,
        InvalidLogConf,
        InvalidArbitrator,
        InvalidNodelist,
        DevNodeHasReqs,
        InvalidResourceId,
        DlvxdNotFound, //WIN 95 ONLY
        NoSuchDevNode,
        NoMoreLogConf,
        NoMoreResDes,
        AlreadySuchDevNode,
        InvalidRangeList,
        InvalidRange,
        Failure,
        NoSuchLogicalDev,
        CreateBlocked,
        NotSystemVm, //WIN 95 ONLY
        RemoveVetoed,
        ApmVetoed,
        InvalidLoadType,
        BufferSmall,
        NoArbitrator,
        NoRegistryHandle,
        RegistryError,
        InvalidDeviceId,
        InvalidData,
        InvalidApi,
        DevLoaderNotReady,
        NeedRestart,
        NoMoreHwProfiles,
        DeviceNotThere,
        NoSuchValue,
        WrongType,
        InvalidPriority,
        NotDisable,
        FreeResources,
        QueryVetoed,
        CantShareIrq,
        NoDependent,
        SameResources,
        NoSuchRegistryKey,
        InvalidMachineName, //NT ONLY
        RemoteCommFailure, //NT ONLY
        MachineUnavailable, //NT ONLY
        NoCmServices, //NT ONLY
        AccessDenied, //NT ONLY
        CallNotImplemented,
        InvalidProperty,
        DeviceInterfaceActive,
        NoSuchDeviceInterface,
        InvalidReferenceString,
        InvalidConflictList,
        InvalidIndex,
        InvalidStructureSize,
        NumCrResults
    }
}