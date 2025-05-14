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
	private TextureRect info1;
	private List<Texture2D> _pages = new();
	private int _currentPageIndex = 0;
	private TextureRect RefPage;
	private TextureRect DbtPage;
	private Button burgerBtn;
	private Button forwardBtn;
	private Button closeBtn;
	private Button DbtBtn;
	private Button DbtOkBtn;
	private Button proponentBtn;
	private TextureRect ourTeam;
	private Button proponentOkBtn;

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
		info1 = GetNode<TextureRect>("LandingScene/Info1");
		RefPage = GetNode<TextureRect>("LandingScene/Ref");
		DbtPage = GetNode<TextureRect>("LandingScene/DBT");
		DbtBtn = GetNode<Button>("LandingScene/DbtBtn");
		DbtOkBtn = GetNode<Button>("LandingScene/DBT/okBtn");
		burgerBtn = GetNode<Button>("LandingScene/burgerBtn");
		forwardBtn = GetNode<Button>("LandingScene/Ref/forwardBtn");
		closeBtn = GetNode<Button>("LandingScene/Ref/closeBtn");
		proponentBtn = GetNode<Button>("LandingScene/proponentBtn");
		ourTeam = GetNode<TextureRect>("LandingScene/OurTeam");
		proponentOkBtn = GetNode<Button>("LandingScene/OurTeam/okBtn");
		
		DbtBtn.Pressed += () => openDbt(true, "res://sprites/DBT/");
		burgerBtn.Pressed += () => openRef(true, "res://sprites/References/");
		forwardBtn.Pressed += () => continuePage("References");
		okBtn.Pressed += () => continuePage("Information");
		DbtOkBtn.Pressed += () => continuePage("DBT");
		closeBtn.Pressed += () => openRef(false);
		proponentBtn.Pressed += () => openProponent(true);
		proponentOkBtn.Pressed += () => openProponent(false);
		
		rateBtn.Pressed += showRatings;
		closeRatings.Pressed += closeRating;
		submitBtn.Pressed += sendRatings;
		infoBtn.Pressed += () => toggleInfo(true, "res://sprites/Information/");
		submitBtn.Disabled = true;
		submitBtn.Visible = false;
		
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
		ratings = new List<string>{"", "", "", "", ""};
		submitBtn.Disabled = true;
		submitBtn.Visible = false;
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
			submitBtn.Visible = true;
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
	
	private void LoadPagesFromFolder(string path)
	{
		_pages.Clear();
		_currentPageIndex = 0;

		// First count how many PNG files exist
		var dir = DirAccess.Open(path);
		if (dir == null)
		{
			GD.PrintErr($"Can't open directory: {path}");
			return;
		}

		int pngCount = 0;
		switch(path){
			case "res://sprites/References/":
				pngCount = 10;
				break;
			case "res://sprites/Information/":
				pngCount = 5;
				break;
			case "res://sprites/DBT/":
				pngCount = 3;
				break;
			default:
				break;
		}
		
		// Then load the pages based on the count
		for (int i = 1; i <= pngCount; i++)
		{
			string filePath = $"{path}{i}.png";
			Texture2D tex = ResourceLoader.Load<Texture2D>(filePath);
			if (tex != null)
				_pages.Add(tex);
			else
				GD.PrintErr($"Failed to load page: {filePath}");
		}
	}
	
	private void updatePage(string type)
	{
		switch(type){
			case "References":
				RefPage.Texture = _pages[_currentPageIndex];
				break;
			case "Information":
				info1.Texture = _pages[_currentPageIndex];
				break;
			case "DBT":
				DbtPage.Texture = _pages[_currentPageIndex];
				break;
			default:
				break;
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

		using (var client = new SmtpClient())
		{
			try
			{
				// ⚡ Add this to skip strict certificate validation (important for Android)
				client.ServerCertificateValidationCallback = (s, c, h, e) => true;

				// Connect to Gmail SMTP
				client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

				// Authenticate
				client.Authenticate("playbiohome@gmail.com", "alha tbng igew rism");

				// Send the email
				client.Send(message);
				GD.Print("Email sent successfully!");
			}
			catch (Exception e)
			{
				GD.PrintErr("Failed to send email: " + e.ToString());
			}
			finally
			{
				client.Disconnect(true);
				client.Dispose();
				ratings = new List<string>{"", "", "", "", ""};
				ratingsPnl.Visible = false;
				submitBtn.Disabled = true;
				submitBtn.Visible = false;
			}
		}
	}
		
	private void sendRatings()
	{
		string message = $"What do they think about the game? {ratings[0]}\nIt helps them be more effective in learning? {ratings[1]}\nIs it easy to use? {ratings[2]}\nDo they learn quickly to use it? {ratings[3]}\nAre they satisfied? {ratings[4]}"; 
		SendEmail("playbiohome@gmail.com", "ezraescalderon@gmail.com", "Anonymous Ratings", message);
		UncheckAllButtons(reactGroup);
		UncheckAllButtons(effectivityGroup);
		UncheckAllButtons(easeGroup);
		UncheckAllButtons(learnGroup);
		UncheckAllButtons(satisfactionGroup);
	}
	
	private void toggleInfo(bool visible1, string path = null)
	{
		if(visible1){
			info1.Position = new Vector2(-132, -166);
			info1.Visible = visible1;
			rateBtn.Disabled = true;
			LoadPagesFromFolder(path);
			updatePage("Information");
		}else{
			rateBtn.Disabled = false;
		}
	}
	
	private void openRef(bool forOpening, string path = null)
	{
		if(forOpening){
			RefPage.Position = new Vector2(-316,-163);
			RefPage.Visible = true;
			LoadPagesFromFolder(path);
			updatePage("References");
		}else{
			RefPage.Visible = false;
		}
	}
	
	private void openDbt(bool forOpening, string path = null)
	{
		if(forOpening){
			DbtPage.Position = new Vector2(-316,-163);
			DbtPage.Visible = true;
			LoadPagesFromFolder(path);
			updatePage("DBT");
		}else{
			DbtPage.Visible = false;
		}
	}
	
	private void openProponent(bool isOpen){
		ourTeam.Position = new Vector2(-195, -204);
		ourTeam.Visible = isOpen;
	}
	
	private void continuePage(string kind)
	{
		if(_currentPageIndex < _pages.Count - 1)
		{
			_currentPageIndex += 1;
			updatePage(kind);
		}else{
			RefPage.Visible = false;
			info1.Visible = false;
			rateBtn.Disabled = false;
			DbtPage.Visible = false;
		}
	}
}
