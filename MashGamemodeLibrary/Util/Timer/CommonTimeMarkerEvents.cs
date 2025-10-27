using LabFusion.Network.Serialization;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;

namespace MashGamemodeLibrary.Util.Timer;

internal class TimeRemainingPacket : INetSerializable
{
    public float TimeRemaining;

    public int? GetSize()
    {
        return sizeof(float);
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref TimeRemaining);
    }
}

public static class CommonTimeMarkerEvents
{
    private static readonly RemoteEvent<TimeRemainingPacket> TimeRemainingEvent = new(OnTimeRemainingEvent, CommonNetworkRoutes.HostToAll);

    private static readonly string[] Ones =
    {
        "Zero",
        "One",
        "Two",
        "Three",
        "Four",
        "Five",
        "Six",
        "Seven",
        "Eight",
        "Nine"
    };

    private static readonly string[] Teens =
    {
        "Ten",
        "Eleven",
        "Twelve",
        "Thirteen",
        "Fourteen",
        "Fifteen",
        "Sixteen",
        "Seventeen",
        "Eighteen",
        "Nineteen"
    };

    private static readonly string[] Tens =
    {
        "",
        "",
        "Twenty",
        "Thirty",
        "Forty",
        "Fifty",
        "Sixty"
    };

    private static string ToText(int number)
    {
        if (number < 10)
            return Ones[number];

        if (number < 20)
            return Teens[number - 10];

        var tensDigit = number / 10;
        var onesDigit = number % 10;

        var tensText = Tens[tensDigit];
        if (onesDigit == 0)
            return tensText;

        return tensText + Ones[onesDigit];
    }

    public static TimeMarker TimeRemaining(float time)
    {
        return new TimeMarker(MarkerType.BeforeEnd, time, _ =>
        {
            Executor.RunIfHost(() =>
            {
                TimeRemainingEvent.Call(new TimeRemainingPacket
                {
                    TimeRemaining = time
                });
            });
        });
    }
        
    // Events

    private static void SendWarning(string text)
    {
        Notifier.Send(new Notification
        {
            Title = text,
            PopupLength = 3f,
            ShowPopup = true,
            SaveToMenu = false,
            Type = NotificationType.WARNING
        });
    }
    
    private static void OnTimeRemainingEvent(TimeRemainingPacket packet)
    {
        if (packet.TimeRemaining >= 60f)
        {
            var minutes = (int)MathF.Round(packet.TimeRemaining / 60f);
            SendWarning($"{ToText(minutes)} Minutes Left");
            return;
        }

        var seconds = (int)MathF.Round(packet.TimeRemaining);
        SendWarning($"{ToText(seconds)} Seconds Left");  
    }
}