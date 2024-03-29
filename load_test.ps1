#!/usr/bin/env pwsh

param(
    [int]$WorkersCount,
    [int]$WorkerShipmentsCount,
    [int]$WorkerDelayMilliseconds
)

function Start-ShipmentProcesses {
    param(
        [int]$WorkersCount,
        [int]$WorkerShipmentsCount,
        [int]$WorkerDelayMilliseconds
    )

    $scriptBlock = {
        param(
            [int]$Count,
            [int]$DelayMilliseconds
        )

        for ($i = 0; $i -lt $Count; $i++) {
            $id = [System.Guid]::NewGuid()
            Invoke-WebRequest -Uri "http://localhost:43210/$id" -Method POST | Out-Null
            Start-Sleep -Milliseconds $DelayMilliseconds
        }
    }

    for ($i = 0; $i -lt $WorkersCount; $i++) {
        Start-Job -Name "ShipmentProcessWorker-$i" -ScriptBlock $scriptBlock -ArgumentList @($WorkerShipmentsCount, $WorkerDelayMilliseconds)
    }

    for ($i = 0; $i -lt $WorkersCount; $i++) {
        Wait-Job -Name "ShipmentProcessWorker-$i"
        Remove-Job -Name "ShipmentProcessWorker-$i"
    }
}

Start-ShipmentProcesses -WorkersCount $WorkersCount -WorkerShipmentsCount $WorkerShipmentsCount -WorkerDelayMilliseconds $WorkerDelayMilliseconds
