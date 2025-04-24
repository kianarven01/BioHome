using Godot;
using System;
using System.Collections.Generic;
using MailKit.Net.Smtp;
using MimeKit;


public partial class MainMenu : Node2D
{
	private AudioStreamPlayer2D musicPlayer;
	private TextureRect ratingsPnl;
	private Button rateBtn;
	private Button closeRatings;
	private Button submitBtn;
	private ButtonGroup react;
	private ButtonGroup ease;
	private ButtonGroup effectivity;
	private ButtonGroup learn;
	private ButtonGroup satisfaction;
	private Control reactGroup;
	private Control easeGroup;
	private Control effectivityGroup;
	private Control learnGroup;
	private Control satisfactionGroup;
	private List<string> ratings;
	private Button infoBtn;
	private Button okBtn;
	private Button proceedBtn;
	private TextureRect info1;
	private TextureRect info2;

	public override void _Ready()
	{
		// Get the AudioStreamPlayer2D node
		musicPlayer = GetNode<AudioStreamPlayer2D>("kahoot_jungle");
		ratingsPnl = GetNode<TextureRect>("LandingScene/RatingPanel");
		rateBtn = GetNode<Button>("LandingScene/rateBtn");
		closeRatings = GetNode<Button>("LandingScene/RatingPanel/close_button");
		submitBtn = GetNode<Button>("LandingScene/RatingPanel/submitBtn");
		ratings = new List<string>{"", "", "", "", ""};
		infoBtn = GetNode<Button>("LandingScene/infoBtn");
		okBtn = GetNode<Button>("LandingScene/Info1/okBtn");
		proceedBtn = GetNode<Button>("LandingScene/Info2/proceedBtn");
		info1 = GetNode<TextureRect>("LandingScene/Info1");
		info2 = GetNode<TextureRect>("LandingScene/Info2");
		
		rateBtn.Pressed += showRatings;
		closeRatings.Pressed += closeRating;
		submitBtn.Pressed += sendRatings;
		infoBtn.Pressed += () => toggleInfo(true, false);
		okBtn.Pressed += () => toggleInfo(false, true);
		proceedBtn.Pressed += () => toggleInfo(false, false);
		submitBtn.Disabled = true;
		
		react =  GD.Load<ButtonGroup>("res://groups/react.tres");
		ease =  GD.Load<ButtonGroup>("res://groups/ease.tres");
		effectivity =  GD.Load<ButtonGroup>("res://groups/effectivity.tres");
		learn =  GD.Load<ButtonGroup>("res://groups/learn.tres");
		satisfaction =  GD.Load<ButtonGroup>("res://groups/satisfaction.tres");
		
		reactGroup = GetNode<Control>("LandingScene/RatingPanel/React");
		effectivityGroup = GetNode<Control>("LandingScene/RatingPanel/Effectivity");
		easeGroup = GetNode<Control>("LandingScene/RatingPanel/Ease");
		learnGroup = GetNode<Control>("LandingScene/RatingPanel/Learn");
		satisfactionGroup = GetNode<Control>("LandingScene/RatingPanel/Satisfaction");
		
		cbAction(react, reactGroup);
		cbAction(effectivity, effectivityGroup);
		cbAction(ease, easeGroup);
		cbAction(learn, learnGroup);
		cbAction(satisfaction, satisfactionGroup);

		// Play the music
		if (!musicPlayer.Playing)
		{
			musicPlayer.Play();
		}
	}
	
	private void showRatings()
	{
		ratingsPnl.Position = new Vector2(-146,-176);
		ratingsPnl.Visible = true;
	}
	
	private void closeRating()
	{
		ratingsPnl.Visible = false;
		UncheckAllButtons(reactGroup);
		UncheckAllButtons(effectivityGroup);
		UncheckAllButtons(easeGroup);
		UncheckAllButtons(learnGroup);
		UncheckAllButtons(satisfactionGroup);
	}

	public override void _Input(InputEvent @event)
	{
		// Handle input events if needed
	}
	
	private void cbAction(ButtonGroup buttonGroup, Control groupParent)
	{
		foreach (Node child in groupParent.GetChildren())
		{
			if (child is CheckButton cb)
			{
				// Force assign the correct ButtonGroup
				cb.ButtonGroup = buttonGroup;

				GD.Print($"Connecting: {cb.Name} to group {buttonGroup.ResourcePath}");
				cb.Toggled += (bool pressed) => OnButtonToggled(pressed, buttonGroup, groupParent);
			}
		}
	}

	private void OnButtonToggled(bool pressed, ButtonGroup buttonGroup, Control groupParent)
	{
		if (!pressed)
			return;

		foreach (Node child in groupParent.GetChildren())
		{
			if (child is CheckButton cb && cb.ButtonGroup == buttonGroup && cb.ButtonPressed)
			{
				GD.Print($"Selected: {cb.Name}");
				identifyGroup(cb);
			}
		}
	}
	
	private void identifyGroup(CheckButton cb){
		List<string> groupDir = new List<string>{"res://groups/react.tres", "res://groups/effectivity.tres", "res://groups/ease.tres", "res://groups/learn.tres", "res://groups/satisfaction.tres"};
		ratings[groupDir.IndexOf(cb.ButtonGroup.ResourcePath)] = cb.Name;
		
		if(!ratings.Contains("")){
			submitBtn.Disabled = false;
		}
	}
	
	private void UncheckAllButtons(Control parent)
	{
		foreach (Node child in parent.GetChildren())
		{
			if (child is CheckButton checkButton)
			{
				checkButton.ButtonPressed = false;
			}
		}
	}

	public void SendEmail(string fromEmail, string toEmail, string subject, string body)
	{
		var message = new MimeMessage();
		message.From.Add(new MailboxAddress("Mr. Potato", fromEmail));
		message.To.Add(new MailboxAddress("Admin", toEmail));
		message.Subject = subject;

		// Create the body of the email
		message.Body = new TextPart("plain")
		{
			Text = body
		};

		// Set up the SMTP client
		using (var client = new SmtpClient())
		{
			try
			{
				// Connect to the SMTP server (example: Gmail)
				client.Connect("smtp.gmail.com", 587, false);
				
				// Authenticate with your email account
				client.Authenticate("gerryvienlifeflores@gmail.com", "xjyh eepp whig hfml");

				// Send the email
				client.Send(message);
				GD.Print("Email sent successfully!");
			}
			catch (Exception e)
			{
				GD.PrintErr("Failed to send email: " + e.Message);
			}
			finally
			{
				// Disconnect from the server
				client.Disconnect(true);
				client.Dispose();
				ratingsPnl.Visible = false;
			}
		}
	}
	
	private void sendRatings()
	{
		string message = $"What do they think about the game? {ratings[0]}\nIt helps them be more effective in learning? {ratings[1]}\nIs it easy to use? {ratings[2]}\nDo they learn quickly to use it? {ratings[3]}\nAre they satisfied? {ratings[4]}"; 
		SendEmail("gerryvienlifeflores@gmail.com", "thisyourman106@gmail.com", "Anonymous Ratings", message);
		UncheckAllButtons(reactGroup);
		UncheckAllButtons(effectivityGroup);
		UncheckAllButtons(easeGroup);
		UncheckAllButtons(learnGroup);
		UncheckAllButtons(satisfactionGroup);
	}
	
	private void toggleInfo(bool visible1, bool visible2)
	{
		info1.Position = new Vector2(-132, -166);
		info2.Position = new Vector2(-132, -166);
		info1.Visible = visible1;
		info2.Visible = visible2;
	}
}
