﻿using Discord.Interactions;
using Discord;

namespace Adramelech.Commands;

public class Feedback : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("feedback", "Send feedback to the developers")]
    public async Task FeedbackAsync()
    {
        var modal = new ModalBuilder()
            .WithTitle("Feedback")
            .WithCustomId("Feedback")
            .AddTextInput("Message", "Message", placeholder: "Please enter your feedback here",
                style: TextInputStyle.Paragraph)
            .Build();

        await RespondWithModalAsync(modal);
    }
}