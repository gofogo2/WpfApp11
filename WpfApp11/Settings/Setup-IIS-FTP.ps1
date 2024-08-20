# ������ �������� �����ؾ� �մϴ�.
# ���� ����
$Username = "ftpuser"
$Password = "1"
$FtpSiteName = "contentsServer"
$FtpRoot = "C:\contents"

# Windows ��� ��ġ �Լ� (DISM ���)
function Install-WindowsFeature {
    $FeatureList = @(
        "IIS-FTPServer",
        "IIS-FTPSvc"
    )
    
    foreach ($Feature in $FeatureList) {
        Write-Host "Installing $Feature..."
        $Result = dism /online /enable-feature /featurename:$Feature /all /norestart
    }
    return $true
}

# ����� ���� ���� �Լ�
function Create-LocalUser {
    param([string]$Username, [string]$Password)
    
    $securePassword = ConvertTo-SecureString $Password -AsPlainText -Force
    New-LocalUser -Name $Username -Password $securePassword -FullName $Username -Description "FTP User" -UserMayNotChangePassword
    Add-LocalGroupMember -Group "Users" -Member $Username
}

# FTP ���丮 ���� �� ���� ���� �Լ�
function Create-FtpDirectory {
    param([string]$Path, [string]$Username)
    
    if (-not (Test-Path $Path)) {
        New-Item -Path $Path -ItemType Directory
    }
    $Acl = Get-Acl $Path
    $Ar = New-Object System.Security.AccessControl.FileSystemAccessRule($Username, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
    $Acl.SetAccessRule($Ar)
    Set-Acl $Path $Acl
}

# FTP ����Ʈ ���� �Լ�
function Create-FtpSite {
    param([string]$SiteName, [string]$PhysicalPath)
    
    Import-Module WebAdministration
    if (-not (Get-Website -Name $SiteName)) {
        New-WebFtpSite -Name $SiteName -PhysicalPath $PhysicalPath -Force
    }
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name ftpServer.security.ssl.controlChannelPolicy -Value 0
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name ftpServer.security.ssl.dataChannelPolicy -Value 0

    # FTP ���� ����
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name ftpServer.security.authentication.basicAuthentication.enabled -Value $true
    
    # ����Ʈ ���ε� ����
    New-WebBinding -Name $SiteName -Protocol ftp -IPAddress "*" -Port 21
}

# FTP ����� ���� ���� �Լ�
function Set-FtpUserPermission {
    param([string]$SiteName, [string]$Username)
    
    $ConfigPath = "IIS:\Sites\$SiteName"
    
    # FTP ���� ���� (�θ� ���ؿ��� ��� ����)
    Set-WebConfigurationProperty -PSPath $ConfigPath -Filter "/system.ftpServer/security/authorization" -Name "overrideMode" -Value "Allow"
    
    Add-WebConfiguration -Filter "/system.ftpServer/security/authorization" -PSPath $ConfigPath -Value @{
        accessType="Allow"
        users=$Username
        permissions="Read,Write"
    }
}

# ���� ���� �κ�
try {
    # FTP ���� ��ġ
    $Success = Install-WindowsFeature
    if (-not $Success) {
        throw "FTP ���� ��ġ�� �����߽��ϴ�."
    }
    
    # ����� ���� ����
    Create-LocalUser -Username $Username -Password $Password
    
    Write-Host "�Ϸ�Ǿ����ϴ�"
} catch {
    Write-Host "���� �߻�: $_"
}


