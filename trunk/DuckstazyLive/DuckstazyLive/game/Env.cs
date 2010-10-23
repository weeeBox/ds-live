using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuckstazyLive.game
{
public class Env
	{
        //// ������ ����������� ��������
        //[Embed(source="gfx/cloud1.png")]
        //private Class rCloudImg1;
        
        //[Embed(source="gfx/cloud2.png")]
        //private Class rCloudImg2;
        
        //[Embed(source="gfx/cloud3.png")]
        //private Class rCloudImg3;
        
        //[Embed(source="gfx/grass.png")]
        //private Class rGrassImg;
        
        //[Embed(source="gfx/grass2.png")]
        //private Class rGrass2Img;
        
        //[Embed(source="gfx/fx_star.png")]
        //private Class rStarImg;
        
        ////[Embed(source="sfx/tex2.mp3")]
        ////private var rTex1Snd:Class;
        
        //[Embed(source="sfx/tex3.mp3")]
        //private Class rTex2Snd;
        
        //[Embed(source="sfx/power.mp3")]
        //private Class rPowerSnd;
                
		private Array imgClouds;
		private Texture2D imgGrass;
		private Texture2D imgGrass2;
		private Texture2D imgGround;
		private Texture2D imgStar;
		private Texture2D imgSky;
		
		private Sound sndPower;
		//private var sndTex1:Sound;
		private Sound sndTex2;
		private Sound music;
		private float musicLenght;
		private SoundChannel channel;
		private SoundTransform musicTrans;
		private float musicAttack;
		public float x;
		public float y;
		
		public float blanc;
		private Shape shBlanc;
		
		// ��������� ���������
		private float power;
		
		// ����� ��� ����������� ���������. ������ ��� ����� ����/����
		public bool day;
		// ����� ��� ������. ������ �������.
		private float time;
		
		
		
		// ������� ��� ������� � ������
		private float grassCounter;		
		
		// ������
		private Array clouds;
		private Array nightSky;
		
		private EnvColor norm;
			
		private const EvnColor[] hell = 
        {
			new EnvColor(0xFF0000, 0xFFFF00), 
			new EnvColor(0xFFFF00, 0xFF0000), 
			new EnvColor(0x00FF00, 0x0000FF),
			new EnvColor(0x00FFFF, 0xFFFF00),
			new EnvColor(0x0000FF, 0xFFFF00),
			new EnvColor(0xFF00FF, 0xFFFF00),
			new EnvColor(0xFF0000, 0xFFFF00)
        };
			
		private const int hellCount = 7;
			
		// ������� �����.
		public EnvColor colors;
		
		public uint colGrass;
		public uint colGround;
		public uint colProgress;
		
		public ColorTransform ctGrass;
		public ColorTransform ctProgress;
				
		// ����-���������� ��� ���������
		private Shape shape;
		
		// �������
		private Array effects;
		private EnvEffect curEffect;
		
		public Env()
		{
			// ��������� ����������.
			float x = 0;
			int i;
			
			shape = new Shape();
			norm = new EnvColor(0x3FB5F2, 0x000000);
			
			// ������� �����
			colors = new EnvColor(0,0);
			
			effects = [new EnvEffect1(), new EnvEffect2(), new EnvEffect3(), new EnvEffect4()];	
			
			shBlanc = new Shape();
			
			blanc = 0.0;
			// �������������� �����
			grassCounter = 0.0;
			
			// �������������� ���������� ���������
			power = 0.0;
			day = true;
			updateNorm();
			time = 0.0;
						
			
			
			// ��������������
			initGrass();
			initDay();
			initNight();
			
			curEffect = effects[3];//effects[int(Math.random()*effects.length)];
			
			colGrass = 0xff00ff00;
			colGround = 0xff371d06;
			colProgress = 0xff5d310c;
			ctGrass = new ColorTransform();
			utils.ARGB2ColorTransform(colGrass, ctGrass);
			ctProgress = new ColorTransform();
			utils.ARGB2ColorTransform(colProgress, ctProgress);
			
			sndPower = new rPowerSnd();
			//sndTex1 = new rTex1Snd();
			sndTex2 = new rTex2Snd();
			
			musicTrans = new SoundTransform(0);
			
			music = sndTex2;
			channel = music.play(0.0, 0, musicTrans);
			channel.addEventListener(Event.SOUND_COMPLETE, loopMusic);
			musicAttack = 0.0;
		}
	
		private void initGrass()
		{
			Rect rc = new Rect(0, 0, 128, 16);
			Texture2D data = (new rGrassImg()).bitmapData;
			Texture2D data2 = (new rGrass2Img()).bitmapData;
			Point dest = new Point();
			int i = 5;
			Matrix mat = new Matrix();
			mat.createGradientBox(640, 80, 1.57, 0, 0);
			
			imgGrass = new Texture2D(640, 8, true, 0x00000000);
			imgGrass2 = new Texture2D(640, 8, true, 0x00000000);
			imgGround = new Texture2D(640, 80, false, 0x00000000); 
			imgGrass.lock();
			imgGrass2.lock();
			
			while(i>0)
			{
				imgGrass.copyPixels(data, rc, dest, null, null, true);
				imgGrass2.copyPixels(data2, rc, dest, null, null, true);
				dest.x+=128.0;
				--i;
			} 

			imgGrass.unlock();
			imgGrass2.unlock();
			
			shape.graphics.clear();
			shape.graphics.beginGradientFill(GradientType.LINEAR, [0x371d06, 0x5d310c], [1.0, 1.0], [0x00, 0xFF], mat);
			shape.graphics.drawRect(0.0, 0.0, 640.0, 80.0);
			shape.graphics.endFill();
			imgGround.draw(shape);
		}
			
		private void initDay()
		{
			float x = 0.0;
			Matrix mat = new Matrix();
			mat.createGradientBox(640, 400, 1.57, 0, 0);
		
			shape.graphics.clear();
			shape.graphics.beginGradientFill(GradientType.LINEAR, [0x3FB5F2, 0xDDF2FF], [1.0, 1.0], [0x00, 0xFF], mat);
			shape.graphics.drawRect(0.0, 0.0, 640.0, 480.0);
			shape.graphics.endFill();
			
			imgSky = new Texture2D(640, 400, false);
			imgSky.draw(shape);
			
			imgClouds = [(new rCloudImg1()).bitmapData, (new rCloudImg2()).bitmapData, (new rCloudImg3()).bitmapData];
			
			clouds = [new EnvCloud(), new EnvCloud(), new EnvCloud(), new EnvCloud(), new EnvCloud()];
			
			foreach (EnvCloud it in clouds)
			{
				it.init(x);
				x+=128.0 + Math.random()*22.0;
			}
			
		}
		
		private void initNight()
		{
			* o;
			int i = 30;
			
			imgStar = (new rStarImg()).bitmapData;

			nightSky = new Array(30);

			while(i>0)
			{
				nightSky[i] = new EnvStar();
				--i;
			}

		}
		
		public void updateNorm()
		{
			if(day)
			{
				norm.bg = 0x3FB5F2;
				norm.text = 0x000000;
			}
			else
			{
				norm.bg = 0x111133;
				norm.text = 0xFFFFFF;
			}
		}
		
		private void updateColors()
		{
			//var pal:EnvColor = new EnvColor(0,0);
			int c;
			float x = time;
			float p2 = power*power;
		
			c = int(x);	x-=c;
			//pal.lerp(x, hell[c], hell[c+1]);
			colors.lerp(x, hell[c], hell[c+1]);
			//colors.lerp(power, norm, pal);
				
			colGrass = 0xff000000|utils.lerpColor(utils.multColorScalar(0x177705, 1.0-p2), colors.bg, p2*grassCounter);
			colGround = 0xff000000|utils.lerpColor(0x371d06, utils.multColorScalar(colors.bg, grassCounter*power), p2);
			colProgress = 0xff000000|utils.lerpColor(0x5d310c, colors.bg, p2);
			
			utils.ARGB2ColorTransform(colGrass, ctGrass);
			utils.ARGB2ColorTransform(colProgress, ctProgress);		
		}
						
		public void update(float dt, float newPower)
		{
			// ��������� ����������.
			float x;
			int i;
			SoundTransform st;
			
			if(newPower!=power)
			{
				if(newPower>=0.5 && power<0.5)
				{
					day = !day;
					updateNorm();
					blanc = 1.0;
					
					/*x = channel.position;
					channel.stop();
					channel.removeEventListener(Event.SOUND_COMPLETE, loopMusic);
					music = sndTex2;
					channel = music.play(x);
					channel.addEventListener(Event.SOUND_COMPLETE, loopMusic);*/
					musicTrans.volume = 1;
					channel.soundTransform = musicTrans;
					
					sndPower.play();
				}
				else if(power>=0.5 && newPower<0.5)
				{
					blanc = 1.0;
					colGrass = 0xff00ff00;
					colGround = 0xff371d06;
					colProgress = 0xff5d310c;
					utils.ARGB2ColorTransform(colGrass, ctGrass);
					utils.ARGB2ColorTransform(colProgress, ctProgress);
					curEffect = effects[int(Math.random()*effects.length)];
					
					/*x = channel.position;
					channel.stop();
					channel.removeEventListener(Event.SOUND_COMPLETE, loopMusic);
					music = sndTex2;
					channel = music.play(x);
					channel.addEventListener(Event.SOUND_COMPLETE, loopMusic);*/
				}
				
				power = newPower;
				
				if(power<0.5)
				{
					musicTrans.volume = power*0.3;
					channel.soundTransform = musicTrans;
				}
			}
			
			/*if(channel.position >= 63900.0)
			{
				st = channel.soundTransform;
				
				channel.stop();
				channel = music.play(0.0, 0, st);
			}*/

			// �������� ������� � ������
			if(grassCounter>0)
			{
				grassCounter-=dt*4.0;
				if(grassCounter<0.0)
					grassCounter = 0.0;
			}
			
			if(power<0.5)
			{
				if(day)
					foreach (EnvCloud c in clouds)
					    c.update(dt, power);
				else
					foreach (EnvStar s in nightSky)
					    s.update(dt, power);
			}
			else
			{
				curEffect.power = power;
				curEffect.c1 = colors.bg;
				curEffect.c2 = utils.multColorScalar(colors.bg, 0.5);
				curEffect.update(dt);
				// ������������ ����� ����/����
				time+=dt*0.1;
				while(time>hellCount-1)
				time-=hellCount-1;
				
				x = (channel.leftPeak + channel.rightPeak)*0.5;
				// ��������� ������� �����
				updateColors();
								
				/*if(musicAttack<x)
					musicAttack = x;
				else if(musicAttack>0.0)
				{
					musicAttack-=dt*10.0;
					if(musicAttack<0.0)
						musicAttack = 0.0;
				}*/
				musicAttack = musicAttack*0.7 + x*0.7;
				
				curEffect.peak = musicAttack;
			}

		}

		public void draw1(bool canvas)
		{
			// ��������� ����������.
			Rect rc = new Rect(0.0, 0.0, 640.0, 400.0);
			Graphics gr = shape.graphics;
			
			
			
			if(power<0.5)
			{
				if(day)
				{
					canvas.copyPixels(imgSky, rc, new Point(0.0, 0.0));
					 
					drawSky(canvas);
				}
				else
				{
					canvas.fillRect(rc, 0x111133);
					drawNight(canvas);
				}
			}
			else
			{
				curEffect.draw(canvas);
				gr.clear();
				gr.beginFill(colors.bg, 0.4*musicAttack);
				gr.drawCircle(613.0 - x, 380.0 - y, musicAttack*30.0);
				gr.drawCircle(320.0 - (x - 293.0)*0.97, 200.0 - (y - 180.0)*0.97, musicAttack*25.0);
				gr.drawCircle(320.0 + (x - 293.0)*0.7, 200.0 + (y - 180.0)*0.7, musicAttack*10.0);
				gr.endFill();
				canvas.draw(shape);
				//canvas.applyFilter(canvas, new Rect(0, 0, 640, 400), new Point(), new ConvolutionFilter(3,3,null, 9));
			}
			
			
		}
		
		public void drawNight(bool canvas)
		{
			// ��������� ����������.
			float x;
			Matrix mat = new Matrix();
			* o;
			EnvStar c;
			
			// ������ ������
			foreach (o in nightSky)
			{
				c = EnvStar(o);
				x = c.t;
								
				mat.identity();
				mat.translate(-7.0, -7.0);
				mat.rotate(c.a);
				mat.scale(0.75 + 0.25*Math.sin(x*6.28), 0.75 + 0.25*Math.sin(x*6.28));
				mat.translate(c.x, c.y);
				
				canvas.draw(imgStar, mat, c.color, null, null, true);
			}
		}
		
		public void drawSky(bool canvas)
		{
			// ��������� ����������.
			float x;
			Matrix mat = new Matrix();
			Texture2D img;
			
			// ������ ������
			foreach (EnvCloud c in clouds)
			{
				x = c.counter;
				img = Texture2D(imgClouds[c.id]); 
				
				mat.identity();
				mat.translate(-img.width*0.5, -img.height*0.5);
				mat.scale(0.9 + 0.1*Math.sin(x*6.28), 0.95 + 0.05*Math.sin(x*6.28 + 3.14));
				mat.translate(c.x, c.y);
				
				canvas.draw(img, mat, null, null, null, true);
			}
		}
	
		public void draw2(bool canvas)
		{
			// ��������� ����������.
			Matrix mat = new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 392.0);
			Rect rc = new Rect(0.0, 400.0, 640.0, 80.0);
			
			/**** ������ ��� ����� ****/
			
			
			
			/**** ����� ****/
			
			if(power<0.5)
			{
				// TODO Optimize
				canvas.draw(imgGround, new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 400.0));
				canvas.draw(imgGrass, mat, ctGrass);
			}
			else
			{
				canvas.fillRect(rc, colGround);
				canvas.draw(imgGrass2, mat, ctGrass);
			}	
		}

		public void beat()
		{
			grassCounter = 1.0;
		}
		
		public void updateBlanc(float dt)
		{
			if(blanc>0.0)
				blanc-=0.5*dt;
		}
		
		public void drawBlanc(bool canvas)
		{
			shBlanc.graphics.clear();
			shBlanc.graphics.beginFill(0xffffff, blanc);
			shBlanc.graphics.drawRect(0.0, 0.0, 640.0, 480.0);
			shBlanc.graphics.endFill();
			canvas.draw(shBlanc);
		}

		public void loopMusic(E e)
		{
			if(power<0.5)
				musicTrans.volume = power*0.3;
				
		    if(channel!=null)
		    {
		    	channel.stop();
		        channel.removeEventListener(Event.SOUND_COMPLETE, loopMusic);
				channel = music.play(0.0, 0, musicTrans);
				channel.addEventListener(Event.SOUND_COMPLETE, loopMusic);
		    }
		}
	}

}
