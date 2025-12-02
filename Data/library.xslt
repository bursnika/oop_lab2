<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" encoding="utf-8" indent="yes"/>

  <xsl:template match="/Library">
    <html>
      <head>
        <meta charset="utf-8"/>
        <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
        <title>Кафедральна бібліотека - Каталог</title>
        <style>
          * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
          }

          body {
            font-family: 'Segoe UI', Arial, sans-serif;
            background: linear-gradient(135deg, #FFF5F7 0%, #F0FFF4 100%);
            min-height: 100vh;
            padding: 20px;
          }

          .container {
            max-width: 1400px;
            margin: 0 auto;
          }

          /* Header */
          .header {
            background: linear-gradient(135deg, #FFB6C1 0%, #98D8C8 100%);
            padding: 40px;
            border-radius: 20px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
            margin-bottom: 30px;
            text-align: center;
          }

          .header h1 {
            color: white;
            font-size: 42px;
            font-weight: bold;
            text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.1);
            margin-bottom: 10px;
          }

          .header p {
            color: white;
            font-size: 16px;
            opacity: 0.95;
          }

          /* Navigation Tabs */
          .nav-tabs {
            display: flex;
            gap: 15px;
            margin-bottom: 30px;
            flex-wrap: wrap;
            justify-content: center;
          }

          .nav-tab {
            background: white;
            padding: 15px 30px;
            border-radius: 12px;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(0, 0, 0, 0.08);
            font-weight: 600;
            font-size: 15px;
            border: 2px solid transparent;
          }

          .nav-tab:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(0, 0, 0, 0.12);
          }

          .nav-tab.books {
            border-color: #FFD1DC;
            color: #FF69B4;
          }

          .nav-tab.books:hover {
            background: linear-gradient(135deg, #FFD1DC, #FFB6C1);
            color: white;
          }

          .nav-tab.readers {
            border-color: #A8E6CF;
            color: #4CAF50;
          }

          .nav-tab.readers:hover {
            background: linear-gradient(135deg, #A8E6CF, #98D8C8);
            color: white;
          }

          .nav-tab.stats {
            border-color: #DDA0DD;
            color: #9370DB;
          }

          .nav-tab.stats:hover {
            background: linear-gradient(135deg, #DDA0DD, #DA70D6);
            color: white;
          }

          /* Sections */
          .section {
            background: white;
            padding: 30px;
            border-radius: 16px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
            margin-bottom: 30px;
          }

          .section-header {
            display: flex;
            align-items: center;
            gap: 15px;
            margin-bottom: 25px;
            padding-bottom: 15px;
            border-bottom: 3px solid #FFD1DC;
          }

          .section-header h2 {
            font-size: 28px;
            color: #333;
            font-weight: bold;
          }

          .section-header .icon {
            font-size: 32px;
          }

          .section-header .count {
            background: linear-gradient(135deg, #FFB6C1, #FF69B4);
            color: white;
            padding: 8px 16px;
            border-radius: 20px;
            font-size: 14px;
            font-weight: bold;
            margin-left: auto;
          }

          /* Book Cards */
          .books-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(380px, 1fr));
            gap: 20px;
          }

          .book-card {
            background: linear-gradient(135deg, #FFF9FB 0%, #FFFFFF 100%);
            border: 2px solid #FFD1DC;
            border-radius: 12px;
            padding: 20px;
            transition: all 0.3s ease;
            cursor: pointer;
            position: relative;
            overflow: hidden;
          }

          .book-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 8px 25px rgba(255, 105, 180, 0.2);
            border-color: #FFB6C1;
          }

          .book-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            width: 5px;
            height: 100%;
            background: linear-gradient(180deg, #FFB6C1 0%, #FF69B4 100%);
          }

          .book-id {
            position: absolute;
            top: 15px;
            right: 15px;
            background: #FFE5EC;
            color: #FF69B4;
            padding: 5px 12px;
            border-radius: 15px;
            font-size: 12px;
            font-weight: bold;
          }

          .book-title {
            font-size: 18px;
            font-weight: bold;
            color: #333;
            margin-bottom: 10px;
            padding-right: 60px;
            line-height: 1.4;
          }

          .book-info {
            display: flex;
            flex-direction: column;
            gap: 8px;
          }

          .book-info-row {
            display: flex;
            align-items: center;
            gap: 8px;
            font-size: 14px;
            color: #666;
          }

          .book-info-row .emoji {
            font-size: 16px;
          }

          .book-info-row .label {
            font-weight: 600;
            color: #333;
          }

          .book-info-row .value {
            color: #666;
          }

          .book-category {
            display: inline-block;
            background: #E8F5E9;
            color: #4CAF50;
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 12px;
            font-weight: 600;
            margin-top: 8px;
          }

          /* Reader Cards */
          .readers-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
            gap: 20px;
          }

          .reader-card {
            background: linear-gradient(135deg, #F0FFF4 0%, #FFFFFF 100%);
            border: 2px solid #A8E6CF;
            border-radius: 12px;
            padding: 20px;
            transition: all 0.3s ease;
            cursor: pointer;
            position: relative;
            overflow: hidden;
          }

          .reader-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 8px 25px rgba(76, 175, 80, 0.2);
            border-color: #98D8C8;
          }

          .reader-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            width: 5px;
            height: 100%;
            background: linear-gradient(180deg, #A8E6CF 0%, #4CAF50 100%);
          }

          .reader-id {
            position: absolute;
            top: 15px;
            right: 15px;
            background: #E8F5E9;
            color: #4CAF50;
            padding: 5px 12px;
            border-radius: 15px;
            font-size: 12px;
            font-weight: bold;
          }

          .reader-name {
            font-size: 18px;
            font-weight: bold;
            color: #333;
            margin-bottom: 12px;
            padding-right: 60px;
          }

          .reader-info {
            display: flex;
            flex-direction: column;
            gap: 8px;
          }

          .reader-info-row {
            display: flex;
            align-items: center;
            gap: 8px;
            font-size: 14px;
            color: #666;
          }

          .reader-info-row .emoji {
            font-size: 16px;
          }

          .reader-info-row .label {
            font-weight: 600;
            color: #333;
          }

          .reader-info-row .value {
            color: #666;
          }

          /* Stats Section */
          .stats-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
          }

          .stat-card {
            background: linear-gradient(135deg, #FFFFFF 0%, #F8F9FA 100%);
            border-radius: 12px;
            padding: 25px;
            text-align: center;
            border: 2px solid #E0E0E0;
            transition: all 0.3s ease;
          }

          .stat-card:hover {
            transform: translateY(-3px);
            box-shadow: 0 6px 20px rgba(0, 0, 0, 0.1);
          }

          .stat-card.pink {
            border-color: #FFD1DC;
            background: linear-gradient(135deg, #FFF9FB 0%, #FFE5EC 100%);
          }

          .stat-card.green {
            border-color: #A8E6CF;
            background: linear-gradient(135deg, #F0FFF4 0%, #E8F5E9 100%);
          }

          .stat-icon {
            font-size: 48px;
            margin-bottom: 10px;
          }

          .stat-value {
            font-size: 36px;
            font-weight: bold;
            color: #333;
            margin-bottom: 5px;
          }

          .stat-label {
            font-size: 14px;
            color: #666;
            font-weight: 600;
          }

          /* Footer */
          .footer {
            text-align: center;
            padding: 30px;
            color: #999;
            font-size: 14px;
            margin-top: 40px;
          }

          /* Smooth scroll */
          html {
            scroll-behavior: smooth;
          }

          /* Responsive */
          @media (max-width: 768px) {
            .header h1 {
              font-size: 28px;
            }

            .books-grid,
            .readers-grid {
              grid-template-columns: 1fr;
            }

            .nav-tabs {
              flex-direction: column;
            }

            .nav-tab {
              width: 100%;
              text-align: center;
            }
          }
        </style>
      </head>
      <body>
        <div class="container">
          <!-- Header -->
          <div class="header">
            <h1>☁️ Кафедральна бібліотека</h1>
            <p>Інформаційна система управління бібліотечним фондом</p>
          </div>

          <!-- Navigation -->
          <div class="nav-tabs">
            <a href="#books" class="nav-tab books">📚 Каталог книг</a>
            <a href="#readers" class="nav-tab readers">👥 Читачі</a>
            <a href="#stats" class="nav-tab stats">📊 Статистика</a>
          </div>

          <!-- Statistics Section -->
          <div id="stats" class="section">
            <div class="section-header">
              <span class="icon">📊</span>
              <h2>Статистика бібліотеки</h2>
            </div>
            <div class="stats-grid">
              <div class="stat-card pink">
                <div class="stat-icon">📚</div>
                <div class="stat-value"><xsl:value-of select="count(Book)"/></div>
                <div class="stat-label">Книг у фонді</div>
              </div>
              <div class="stat-card green">
                <div class="stat-icon">👥</div>
                <div class="stat-value"><xsl:value-of select="count(Reader)"/></div>
                <div class="stat-label">Активних читачів</div>
              </div>
              <div class="stat-card pink">
                <div class="stat-icon">📂</div>
                <div class="stat-value"><xsl:value-of select="count(Book[not(normalize-space(category) = preceding::Book/category)])"/></div>
                <div class="stat-label">Категорій</div>
              </div>
              <div class="stat-card green">
                <div class="stat-icon">🎓</div>
                <div class="stat-value"><xsl:value-of select="count(Reader[not(normalize-space(@faculty) = preceding::Reader/@faculty)])"/></div>
                <div class="stat-label">Факультетів</div>
              </div>
            </div>
          </div>

          <!-- Books Section -->
          <div id="books" class="section">
            <div class="section-header">
              <span class="icon">📚</span>
              <h2>Каталог книг</h2>
              <span class="count"><xsl:value-of select="count(Book)"/> книг</span>
            </div>
            <div class="books-grid">
              <xsl:for-each select="Book">
                <xsl:sort select="@title"/>
                <div class="book-card">
                  <div class="book-id">ID: <xsl:value-of select="@id"/></div>
                  <div class="book-title">
                    📖 <xsl:value-of select="@title"/>
                  </div>
                  <div class="book-info">
                    <div class="book-info-row">
                      <span class="emoji">✍️</span>
                      <span class="label">Автор:</span>
                      <span class="value"><xsl:value-of select="@author"/></span>
                    </div>
                    <div class="book-info-row">
                      <span class="emoji">👤</span>
                      <span class="label">Читач:</span>
                      <span class="value"><xsl:value-of select="@reader"/></span>
                    </div>
                    <div class="book-info-row">
                      <span class="emoji">📅</span>
                      <span class="label">Рік:</span>
                      <span class="value"><xsl:value-of select="@year"/></span>
                    </div>
                    <div class="book-info-row">
                      <span class="emoji">📚</span>
                      <span class="label">ISBN:</span>
                      <span class="value"><xsl:value-of select="@isbn"/></span>
                    </div>
                  </div>
                  <div class="book-category">
                    📂 <xsl:value-of select="@category"/>
                  </div>
                </div>
              </xsl:for-each>
            </div>
          </div>

          <!-- Readers Section -->
          <div id="readers" class="section">
            <div class="section-header">
              <span class="icon">👥</span>
              <h2>Читачі бібліотеки</h2>
              <span class="count" style="background: linear-gradient(135deg, #A8E6CF, #4CAF50);"><xsl:value-of select="count(Reader)"/> читачів</span>
            </div>
            <div class="readers-grid">
              <xsl:for-each select="Reader">
                <xsl:sort select="@fullName"/>
                <div class="reader-card">
                  <div class="reader-id">ID: <xsl:value-of select="@id"/></div>
                  <div class="reader-name">
                    👤 <xsl:value-of select="@fullName"/>
                  </div>
                  <div class="reader-info">
                    <div class="reader-info-row">
                      <span class="emoji">🎓</span>
                      <span class="label">Факультет:</span>
                      <span class="value"><xsl:value-of select="@faculty"/></span>
                    </div>
                    <div class="reader-info-row">
                      <span class="emoji">📊</span>
                      <span class="label">Курс:</span>
                      <span class="value"><xsl:value-of select="@course"/></span>
                    </div>
                    <div class="reader-info-row">
                      <span class="emoji">📧</span>
                      <span class="label">Email:</span>
                      <span class="value"><xsl:value-of select="@email"/></span>
                    </div>
                  </div>
                </div>
              </xsl:for-each>
            </div>
          </div>

          <!-- Footer -->
          <div class="footer">
            <p>📚 Кафедральна бібліотека | Система управління бібліотечним фондом</p>
            <p style="margin-top: 10px;">Згенеровано автоматично з XML</p>
          </div>
        </div>
      </body>
    </html>
  </xsl:template>

</xsl:stylesheet>
