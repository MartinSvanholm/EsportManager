using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace EsportManager.Features.Credentials;

public static class WindowsCredentialManager
{
    private const int ErrorNotFound = 1168;
    private const uint PersistLocalMachine = 2;

    public enum CredentialType : uint
    {
        Generic = 1,
        DomainPassword = 2
    }

    public static (string UserName, string Password)? Read(string target, CredentialType type)
    {
        if (!CredRead(target, type, 0, out IntPtr credentialPtr))
        {
            int error = Marshal.GetLastWin32Error();
            if (error == ErrorNotFound)
            {
                return null;
            }

            throw new Win32Exception(error);
        }

        try
        {
            var credential = Marshal.PtrToStructure<Credential>(credentialPtr);
            string password = credential.CredentialBlobSize > 0
                ? Marshal.PtrToStringUni(credential.CredentialBlob, (int)credential.CredentialBlobSize / sizeof(char))
                : string.Empty;

            return (credential.UserName ?? string.Empty, password);
        }
        finally
        {
            CredFree(credentialPtr);
        }
    }

    public record StoredCredential(string TargetName, CredentialType Type, string? Comment);

    public static List<StoredCredential> Enumerate()
    {
        if (!CredEnumerate(null, 0, out int count, out IntPtr credentialsPtr))
        {
            int error = Marshal.GetLastWin32Error();
            if (error == ErrorNotFound)
            {
                return [];
            }

            throw new Win32Exception(error);
        }

        try
        {
            List<StoredCredential> credentials = [];
            for (int i = 0; i < count; i++)
            {
                IntPtr credentialPtr = Marshal.ReadIntPtr(credentialsPtr, i * IntPtr.Size);
                var credential = Marshal.PtrToStructure<Credential>(credentialPtr);

                credentials.Add(new StoredCredential(credential.TargetName, credential.Type, credential.Comment));
            }

            return credentials;
        }
        finally
        {
            CredFree(credentialsPtr);
        }
    }

    public static void Write(string target, CredentialType type, string userName, string password, string? comment = null)
    {
        byte[] blob = Encoding.Unicode.GetBytes(password);
        IntPtr blobPtr = Marshal.AllocHGlobal(blob.Length);

        try
        {
            Marshal.Copy(blob, 0, blobPtr, blob.Length);

            var credential = new Credential
            {
                Type = type,
                TargetName = target,
                Comment = comment,
                UserName = userName,
                CredentialBlob = blobPtr,
                CredentialBlobSize = (uint)blob.Length,
                Persist = PersistLocalMachine
            };

            if (!CredWrite(ref credential, 0))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
        finally
        {
            Marshal.Copy(new byte[blob.Length], 0, blobPtr, blob.Length);
            Marshal.FreeHGlobal(blobPtr);
        }
    }

    public static void Delete(string target, CredentialType type)
    {
        if (!CredDelete(target, type, 0))
        {
            int error = Marshal.GetLastWin32Error();
            if (error != ErrorNotFound)
            {
                throw new Win32Exception(error);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct Credential
    {
        public uint Flags;
        public CredentialType Type;
        public string TargetName;
        public string? Comment;
        public FILETIME LastWritten;
        public uint CredentialBlobSize;
        public IntPtr CredentialBlob;
        public uint Persist;
        public uint AttributeCount;
        public IntPtr Attributes;
        public string? TargetAlias;
        public string? UserName;
    }

    [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredRead(string target, CredentialType type, int reservedFlag, out IntPtr credential);

    [DllImport("advapi32.dll", EntryPoint = "CredEnumerateW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredEnumerate(string? filter, int flags, out int count, out IntPtr credentials);

    [DllImport("advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredWrite(ref Credential credential, uint flags);

    [DllImport("advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredDelete(string target, CredentialType type, int flags);

    [DllImport("advapi32.dll")]
    private static extern void CredFree(IntPtr buffer);
}
