node {
	stage('Checkout') {
		checkout scm
	}

	stage('Acquire SE') {
		bat 'powershell -File jenkins-grab-se.ps1'
		bat 'IF EXIST GameBinaries RMDIR GameBinaries'
		bat 'mklink /J GameBinaries "C:/Steam/Data/DedicatedServer64/"'		
	}

	stage('Acquire NuGet Packages') {
		bat 'nuget restore Torch.sln'
	}

	stage('Build') {
		bat "\"${tool 'MSBuild'}msbuild\" Torch.sln /p:Configuration=Release /p:Platform=x64"
	}

	stage('Test') {
		bat 'IF NOT EXIST reports MKDIR reports'
		bat "\"packages/xunit.runner.console.2.2.0/tools/xunit.console.exe\" \"bin-test/x64/Release/Torch.Tests.dll\" \"bin-test/x64/Release/Torch.Server.Tests.dll\" \"bin-test/x64/Release/Torch.Client.Tests.dll\" -parallel none -xml \"reports/Torch.Tests.xml\""
	    step([
	        $class: 'XUnitBuilder',
	        thresholdMode: 1,
	        thresholds: [[$class: 'FailedThreshold', failureThreshold: '1']],
	        tools: [[
	            $class: 'XUnitDotNetTestType',
	            deleteOutputFiles: true,
	            failIfNotNew: true,
	            pattern: 'reports/*.xml',
	            skipNoTestFiles: false,
	            stopProcessingIfError: true
	        ]]
	    ])
	}

	stage('Archive') {
		archive 'bin/x64/Release/Torch.*'
	}
}
